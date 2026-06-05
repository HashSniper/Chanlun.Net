class ChanLunDataFeed {
    constructor(options) {
        this.apiUrl = (options.apiUrl || 'http://localhost:5000').replace(/\/$/, '');
        this.symbol = options.symbol || 'SH600000';
        this.widget = null;
    }

    setChartWidget(widget) {
        this.widget = widget;
    }

    onReady(callback) {
        fetch(`${this.apiUrl}/api/tradingview/config`)
            .then(r => r.json())
            .then(config => {
                callback({
                    supports_search: config.supports_search ?? true,
                    supports_group_request: config.supports_group_request ?? false,
                    supports_marks: config.supports_marks ?? false,
                    supports_timescale_marks: config.supports_timescale_marks ?? false,
                    supports_time: config.supports_time ?? true,
                    exchanges: config.exchanges ?? [{ value: "SH", name: "上海证券交易所", desc: "上证" }],
                    symbols_types: config.symbols_types ?? [{ name: "股票", value: "stock" }],
                    supported_resolutions: config.supported_resolutions ?? ["1", "5", "15", "30", "60", "240", "D", "W", "M"]
                });
            })
            .catch(() => {
                callback({
                    supports_search: true,
                    supports_group_request: false,
                    supports_marks: false,
                    supports_timescale_marks: false,
                    supports_time: true,
                    exchanges: [{ value: "SH", name: "上海证券交易所", desc: "上证" }, { value: "SZ", name: "深圳证券交易所", desc: "深证" }],
                    symbols_types: [{ name: "股票", value: "stock" }],
                    supported_resolutions: ["1", "5", "15", "30", "60", "240", "D", "W", "M"]
                });
            });
    }

    resolveSymbol(symbolName, onSymbolResolvedCallback, onResolveErrorCallback) {
        fetch(`${this.apiUrl}/api/tradingview/symbols?symbol=${encodeURIComponent(symbolName)}`)
            .then(r => r.json())
            .then(info => {
                onSymbolResolvedCallback({
                    name: info.name || symbolName,
                    description: info.description || symbolName,
                    type: info.type || "stock",
                    session: info.session || "0900-1130,1300-1500",
                    exchange: info.exchange || "SH",
                    listed_exchange: info.listed_exchange || "SH",
                    timezone: info.timezone || "Asia/Shanghai",
                    pricescale: parseInt(info.pricescale) || 100,
                    minmov: info.minmov || 1,
                    minmove2: info.minmove2 || 0,
                    has_intraday: info.has_intraday ?? true,
                    supported_resolutions: info.supported_resolutions || ["1", "5", "15", "30", "60", "240", "D", "W", "M"],
                    has_daily: info.has_daily ?? true,
                    has_weekly_and_monthly: info.has_weekly_and_monthly ?? true,
                    has_empty_bars: info.has_empty_bars ?? false,
                    force_session_rebuild: info.force_session_rebuild ?? true,
                });
            })
            .catch(err => {
                console.warn('[ChanLun] resolveSymbol fallback:', err);
                onSymbolResolvedCallback({
                    name: symbolName,
                    description: symbolName,
                    type: "stock",
                    session: "0900-1130,1300-1500",
                    exchange: "SH",
                    listed_exchange: "SH",
                    timezone: "Asia/Shanghai",
                    pricescale: 100,
                    minmov: 1,
                    has_intraday: true,
                    supported_resolutions: ["1", "5", "15", "30", "60", "240", "D", "W", "M"],
                    has_daily: true,
                    has_weekly_and_monthly: true,
                });
            });
    }

    getBars(symbolInfo, resolution, periodParams, onHistoryCallback, onErrorCallback) {
        const { from, to, countBack, firstDataRequest } = periodParams;

        fetch(`${this.apiUrl}/api/tradingview/history?symbol=${encodeURIComponent(symbolInfo.name)}&resolution=${resolution}&from=${from}&to=${to}`)
            .then(r => r.json())
            .then(data => {
                if (data.s === 'no_data') {
                    onHistoryCallback([], { noData: true });
                    return;
                }
                if (data.s === 'error') {
                    onErrorCallback(data.errmsg || 'Unknown error');
                    return;
                }

                const bars = [];
                for (let i = 0; i < data.t.length; i++) {
                    bars.push({
                        time: data.t[i] * 1000,
                        open: data.o[i],
                        high: data.h[i],
                        low: data.l[i],
                        close: data.c[i],
                        volume: data.v ? data.v[i] : 0
                    });
                }

                onHistoryCallback(bars, { noData: false });

                if (firstDataRequest && this.widget) {
                    this.loadChanlunOverlay(symbolInfo.name, bars);
                }
            })
            .catch(err => {
                console.error('[ChanLun] getBars error:', err);
                onErrorCallback(err.message);
            });
    }

    searchSymbols(userInput, exchange, symbolType, onResultReadyCallback) {
        fetch(`${this.apiUrl}/api/tradingview/search?query=${encodeURIComponent(userInput)}&limit=30`)
            .then(r => r.json())
            .then(results => {
                const mapped = (results || []).map(item => ({
                    symbol: item.symbol,
                    full_name: item.full_name || `${item.exchange}:${item.symbol}`,
                    description: item.description || item.symbol,
                    exchange: item.exchange || "SH",
                    type: item.type || "stock",
                }));
                onResultReadyCallback(mapped);
            })
            .catch(() => onResultReadyCallback([]));
    }

    subscribeBars(symbolInfo, resolution, onRealtimeCallback, subscriberUID, onResetCacheNeededCallback) {
    }

    unsubscribeBars(subscriberUID) {
    }

    getServerTime(callback) {
        fetch(`${this.apiUrl}/api/tradingview/time`)
            .then(r => r.text())
            .then(t => callback(parseInt(t) * 1000))
            .catch(() => callback(Date.now()));
    }

    async loadChanlunOverlay(symbol, bars) {
        if (!this.widget) return;
        
        try {
            console.log('[ChanLun] Loading overlay for', symbol);
            
            const apiBars = bars.map(b => ({
                time: new Date(b.time).toISOString(),
                open: b.open,
                high: b.high,
                low: b.low,
                close: b.close,
                volume: b.volume || 0
            }));

            const resp = await fetch(`${this.apiUrl}/api/calculation/tvchanlun`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ symbol, bars: apiBars })
            });

            if (!resp.ok) {
                console.warn('[ChanLun] Overlay calculation failed:', resp.status);
                return;
            }

            const data = await resp.json();
            this.drawOverlay(data);
        } catch (err) {
            console.error('[ChanLun] Overlay error:', err);
        }
    }

    drawOverlay(data) {
        if (!this.widget) return;
        const chart = this.widget.activeChart();

        if (data.biList) {
            data.biList.forEach(bi => {
                const color = bi.direction === 'up' ? '#ff6d00' : '#00c853';
                chart.createMultipointShape(
                    [
                        { time: bi.startTime / 1000, price: bi.startPrice },
                        { time: bi.endTime / 1000, price: bi.endPrice }
                    ],
                    {
                        shape: 'trend_line',
                        overrides: {
                            linecolor: color,
                            linewidth: 2,
                            linestyle: 0
                        }
                    }
                );
            });
        }

        if (data.segList) {
            data.segList.forEach(seg => {
                const color = seg.direction === 'up' ? '#d50000' : '#00bfa5';
                chart.createMultipointShape(
                    [
                        { time: seg.startTime / 1000, price: seg.startPrice },
                        { time: seg.endTime / 1000, price: seg.endPrice }
                    ],
                    {
                        shape: 'trend_line',
                        overrides: {
                            linecolor: color,
                            linewidth: 3,
                            linestyle: 0
                        }
                    }
                );
            });
        }

        const drawPivot = (pivotList, color) => {
            if (!pivotList) return;
            pivotList.forEach(pivot => {
                chart.createMultipointShape(
                    [
                        { time: pivot.startTime / 1000, price: pivot.zg },
                        { time: pivot.endTime / 1000, price: pivot.zd }
                    ],
                    {
                        shape: 'rectangle',
                        overrides: {
                            linecolor: color,
                            linewidth: 1,
                            fillBackground: true,
                            backgroundColor: color.replace(')', ', 0.15)').replace('rgb', 'rgba'),
                            transparency: 80
                        }
                    }
                );
            });
        };

        drawPivot(data.biPivotList, 'rgb(41, 98, 255)');
        drawPivot(data.segPivotList, 'rgb(255, 171, 0)');

        console.log('[ChanLun] Overlay drawn:', {
            bi: data.biList?.length || 0,
            seg: data.segList?.length || 0,
            biPivot: data.biPivotList?.length || 0,
            segPivot: data.segPivotList?.length || 0
        });
    }
}
