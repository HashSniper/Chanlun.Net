const { createChart, LineStyle } = LightweightCharts;

let chart = null;
let candleSeries = null;
let overlaySeries = [];
let currentBars = [];

function initChart() {
    const chartContainer = document.getElementById('chart');
    chart = createChart(chartContainer, {
        layout: {
            background: { type: 'solid', color: '#131722' },
            textColor: '#d1d4dc',
        },
        grid: {
            vertLines: { color: '#2a2e39' },
            horzLines: { color: '#2a2e39' },
        },
        crosshair: {
            mode: LightweightCharts.CrosshairMode.Normal,
        },
        rightPriceScale: {
            borderColor: '#2a2e39',
        },
        timeScale: {
            borderColor: '#2a2e39',
            timeVisible: true,
            secondsVisible: false,
        },
        autoSize: true,
    });

    candleSeries = chart.addCandlestickSeries({
        upColor: '#26a69a',
        downColor: '#ef5350',
        borderVisible: false,
        wickUpColor: '#26a69a',
        wickDownColor: '#ef5350',
    });

    const resizeObserver = new ResizeObserver(() => {
        chart.applyOptions({
            width: chartContainer.clientWidth,
            height: chartContainer.clientHeight,
        });
    });
    resizeObserver.observe(chartContainer);
}

function clearOverlays() {
    overlaySeries.forEach(s => {
        try { chart.removeSeries(s); } catch (e) {}
    });
    overlaySeries = [];
}

function setStatus(text, type) {
    const el = document.getElementById('status');
    el.textContent = text;
    el.className = 'status ' + type;
}

function msToSec(ms) {
    return Math.floor(ms / 1000);
}

function addSeries(options) {
    const s = chart.addLineSeries(options);
    overlaySeries.push(s);
    return s;
}

function addBaselineSeries(options) {
    const s = chart.addBaselineSeries(options);
    overlaySeries.push(s);
    return s;
}

function renderBi(biList) {
    if (!biList || biList.length === 0) return;
    biList.forEach(bi => {
        const color = bi.direction === 'up' ? '#ff6d00' : '#00c853';
        const series = addSeries({
            color: color,
            lineWidth: 2,
            lastValueVisible: false,
            priceLineVisible: false,
            crosshairMarkerVisible: false,
        });
        series.setData([
            { time: msToSec(bi.startTime), value: bi.startPrice },
            { time: msToSec(bi.endTime), value: bi.endPrice }
        ]);
    });
}

function renderSeg(segList) {
    if (!segList || segList.length === 0) return;
    segList.forEach(seg => {
        const color = seg.direction === 'up' ? '#d50000' : '#00bfa5';
        const series = addSeries({
            color: color,
            lineWidth: 3,
            lastValueVisible: false,
            priceLineVisible: false,
            crosshairMarkerVisible: false,
        });
        series.setData([
            { time: msToSec(seg.startTime), value: seg.startPrice },
            { time: msToSec(seg.endTime), value: seg.endPrice }
        ]);
    });
}

function renderPivots(pivotList, colorBase, alpha) {
    if (!pivotList || pivotList.length === 0) return;
    pivotList.forEach(pivot => {
        const color = colorBase + alpha + ')';
        const fill1 = colorBase + Math.min(alpha * 1.2, 0.6) + ')';
        const fill2 = colorBase + '0.05)';

        const series = addBaselineSeries({
            baseValue: { type: 'price', price: pivot.zd },
            topLineColor: color,
            topFillColor1: fill1,
            topFillColor2: fill2,
            bottomLineColor: 'transparent',
            bottomFillColor1: 'transparent',
            bottomFillColor2: 'transparent',
            lineWidth: 1,
            lastValueVisible: false,
            priceLineVisible: false,
            crosshairMarkerVisible: false,
        });
        series.setData([
            { time: msToSec(pivot.startTime), value: pivot.zg },
            { time: msToSec(pivot.endTime), value: pivot.zg }
        ]);
    });
}

function renderMergedKLines(klines) {
    if (!klines || klines.length === 0) return;
    klines.forEach(kl => {
        const color = kl.direction === 'up' ? 'rgba(38, 166, 154, 0.25)' :
                      kl.direction === 'down' ? 'rgba(239, 83, 80, 0.25)' : 'rgba(150, 150, 150, 0.15)';

        const series = addBaselineSeries({
            baseValue: { type: 'price', price: kl.low },
            topLineColor: 'transparent',
            topFillColor1: color,
            topFillColor2: color,
            bottomLineColor: 'transparent',
            bottomFillColor1: 'transparent',
            bottomFillColor2: 'transparent',
            lineWidth: 1,
            lastValueVisible: false,
            priceLineVisible: false,
            crosshairMarkerVisible: false,
        });
        series.setData([
            { time: msToSec(kl.startTime), value: kl.high },
            { time: msToSec(kl.endTime), value: kl.high }
        ]);
    });
}

async function calculate() {
    const apiUrl = document.getElementById('apiUrl').value.trim().replace(/\/$/, '');
    const symbol = document.getElementById('symbol').value.trim();
    const source = document.getElementById('dataSource').value;
    const btn = document.getElementById('btnCalc');

    btn.disabled = true;
    setStatus('计算中...', 'loading');
    clearOverlays();

    try {
        let bars = [];

        if (source === 'manual') {
            const text = document.getElementById('klineData').value.trim();
            if (!text) throw new Error('请输入K线JSON数据');
            bars = JSON.parse(text);
        } else {
            const now = Math.floor(Date.now() / 1000);
            const from = now - 86400 * 365;
            const resp = await fetch(`${apiUrl}/api/tradingview/history?symbol=${encodeURIComponent(symbol)}&resolution=D&from=${from}&to=${now}`);
            const data = await resp.json();
            if (data.s === 'no_data') throw new Error('API缓存中无该symbol的K线数据，请先调用push接口推送数据');
            if (data.s === 'error') throw new Error(data.errmsg || '获取历史数据失败');
            if (!data.t || data.t.length === 0) throw new Error('无历史数据');

            bars = data.t.map((t, i) => ({
                time: new Date(t * 1000).toISOString(),
                open: data.o[i],
                high: data.h[i],
                low: data.l[i],
                close: data.c[i],
                volume: data.v ? data.v[i] : 0
            }));
            document.getElementById('klineData').value = JSON.stringify(bars, null, 2);
        }

        if (!Array.isArray(bars) || bars.length === 0) {
            throw new Error('K线数据为空或格式不正确');
        }

        currentBars = bars;

        // 渲染K线
        const candleData = bars.map(b => {
            const t = typeof b.time === 'number' ? Math.floor(b.time / 1000) : msToSec(new Date(b.time).getTime());
            return {
                time: t,
                open: b.open,
                high: b.high,
                low: b.low,
                close: b.close
            };
        }).sort((a, b) => a.time - b.time);

        candleSeries.setData(candleData);

        // 调用缠论计算API
        const resp = await fetch(`${apiUrl}/api/calculation/tvchanlun`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ symbol, bars })
        });

        if (!resp.ok) {
            const err = await resp.json().catch(() => ({}));
            throw new Error(err.error || `HTTP ${resp.status}`);
        }

        const data = await resp.json();

        // 渲染各个图层
        if (document.getElementById('showBi').checked) renderBi(data.biList);
        if (document.getElementById('showSeg').checked) renderSeg(data.segList);
        if (document.getElementById('showBiPivot').checked) renderPivots(data.biPivotList, 'rgba(41, 98, 255, ', 0.35);
        if (document.getElementById('showSegPivot').checked) renderPivots(data.segPivotList, 'rgba(255, 171, 0, ', 0.35);
        if (document.getElementById('showMergedKLine').checked) renderMergedKLines(data.mergedKLines);

        chart.timeScale().fitContent();

        setStatus(`✅ 完成 | 笔:${data.biList?.length||0} 线段:${data.segList?.length||0} 笔中枢:${data.biPivotList?.length||0} 线段中枢:${data.segPivotList?.length||0}`, 'ok');
    } catch (err) {
        setStatus('❌ ' + err.message, 'error');
        console.error(err);
    } finally {
        btn.disabled = false;
    }
}

// 初始化
document.addEventListener('DOMContentLoaded', () => {
    initChart();
});
