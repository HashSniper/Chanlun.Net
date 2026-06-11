import { useEffect, useRef, useState, useCallback } from 'react';
import {
  createChart,
  CandlestickSeries,
  LineSeries,
  BaselineSeries,
  HistogramSeries,
  createSeriesMarkers,
  type IChartApi,
  type ISeriesApi,
  type CandlestickData,
  type Time,
  type SeriesMarker,
  type ISeriesPrimitive,
} from 'lightweight-charts';
import type { ChanlunResponse, KlineBar } from '../types/chanlun';

interface TooltipData {
  time: string;
  open: number;
  high: number;
  low: number;
  close: number;
  change: number;
  changePct: number;
  x: number;
  y: number;
}

interface PatternModalData {
  time: string;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
  patterns: string[];
  patternDirection: string;
  patternSignal: string;
  x: number;
  y: number;
  // 量能关系指标
  volumeRatio5?: number;
  volumeChangePct?: number;
  obv?: number;
  volumeSignal?: string;
  volumeBullish?: boolean | null;

  // 海龟交易法则指标
  turtleHigh20?: number;
  turtleHigh50?: number;
  turtleLow20?: number;
  turtleLow50?: number;
  turtleBreakoutHigh20?: boolean;
  turtleBreakoutHigh50?: boolean;
  turtleBreakdownLow20?: boolean;
  turtleBreakdownLow50?: boolean;
  turtleSignal?: string;
  turtleBullish?: boolean | null;

  // KDJ 指标基础值
  kdjK?: number;
  kdjD?: number;
  kdjJ?: number;

  // KDJ 指标信号
  kdjSignal?: string;
  kdjBullish?: boolean | null;
  kdjGoldenCross?: boolean;
  kdjDeathCross?: boolean;
  kdjBottomDivergence?: boolean;
  kdjTopDivergence?: boolean;
}

interface Props {
  klines: KlineBar[];
  chanlun: ChanlunResponse | null;
  showBi: boolean;
  showSeg: boolean;
  showBiPivot: boolean;
  showSegPivot: boolean;
  showMergedKLine: boolean;
}

function msToSec(ms: number): number {
  return Math.floor(ms / 1000);
}

function formatTime(sec: number): string {
  const d = new Date(sec * 1000);
  return d.toLocaleString('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
}

function formatDateTime(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleString('zh-CN', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  });
}

function setPaneHeights(chart: IChartApi, containerHeight: number) {
  const panes = chart.panes();
  if (panes.length >= 2) {
    panes[0].setHeight(Math.floor(containerHeight * 0.7));
    panes[1].setHeight(Math.floor(containerHeight * 0.3));
  }
}

class PaneSeparator implements ISeriesPrimitive<Time> {
  paneViews() {
    return [
      {
        zOrder: () => 'top' as const,
        renderer: () => ({
          draw: (target: any) => {
            target.useBitmapCoordinateSpace(({ context, bitmapSize }: any) => {
              const y = bitmapSize.height - 1;
              const w = bitmapSize.width;
              context.save();
              const gradient = context.createLinearGradient(0, 0, w, 0);
              gradient.addColorStop(0, 'rgba(90, 94, 105, 0)');
              gradient.addColorStop(0.1, 'rgba(90, 94, 105, 1)');
              gradient.addColorStop(0.5, 'rgba(138, 142, 153, 1)');
              gradient.addColorStop(0.9, 'rgba(90, 94, 105, 1)');
              gradient.addColorStop(1, 'rgba(90, 94, 105, 0)');
              context.strokeStyle = gradient;
              context.lineWidth = 1;
              context.shadowColor = 'rgba(138, 142, 153, 0.35)';
              context.shadowBlur = 6;
              context.beginPath();
              context.moveTo(0, y);
              context.lineTo(w, y);
              context.stroke();
              context.restore();
            });
          },
        }),
      },
    ];
  }
}

/** 在 pane 1 顶部绘制"指标计算结果"标题，天然位于分界线下方 */
class PaneTitlePrimitive implements ISeriesPrimitive<Time> {
  private _title: string;

  constructor(title: string) {
    this._title = title;
  }

  paneViews() {
    return [
      {
        zOrder: () => 'top' as const,
        renderer: () => ({
          draw: (target: any) => {
            target.useBitmapCoordinateSpace(({ context }: any) => {
              context.save();
              context.font = '11px -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
              context.fillStyle = '#868993';
              context.textAlign = 'left';
              context.textBaseline = 'top';
              context.fillText(this._title, 4, 4);
              context.restore();
            });
          },
        }),
      },
    ];
  }
}

export default function ChanLunChart({
  klines,
  chanlun,
  showBi,
  showSeg,
  showBiPivot,
  showSegPivot,
  showMergedKLine,
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const chartRef = useRef<IChartApi | null>(null);
  const seriesRefs = useRef<ISeriesApi<any>[]>([]);
  const markerPluginsRef = useRef<{ detach: () => void }[]>([]);
  const candleSeriesRef = useRef<ISeriesApi<'Candlestick'> | null>(null);
  const [tooltip, setTooltip] = useState<TooltipData | null>(null);
  const [modal, setModal] = useState<PatternModalData | null>(null);

  // Init chart
  useEffect(() => {
    if (!containerRef.current) return;
    const chart = createChart(containerRef.current, {
      layout: {
        background: { type: 'solid' as any, color: '#131722' },
        textColor: '#d1d4dc',
      },
      grid: {
        vertLines: { color: '#2a2e39' },
        horzLines: { color: '#2a2e39' },
      },
      crosshair: { mode: 0 },
      rightPriceScale: { borderColor: '#2a2e39' },
      timeScale: { borderColor: '#2a2e39', timeVisible: true, secondsVisible: false },
      autoSize: true,
    });
    chartRef.current = chart;

    chart.addPane();
    if (containerRef.current) {
      setPaneHeights(chart, containerRef.current.clientHeight);
    }

    const resizeObserver = new ResizeObserver(() => {
      if (containerRef.current) {
        const h = containerRef.current.clientHeight;
        chart.applyOptions({ width: containerRef.current.clientWidth, height: h });
        setPaneHeights(chart, h);
      }
    });
    resizeObserver.observe(containerRef.current);

    return () => {
      resizeObserver.disconnect();
      chart.remove();
      chartRef.current = null;
      candleSeriesRef.current = null;
    };
  }, []);

  const clearOverlays = useCallback(() => {
    markerPluginsRef.current.forEach((p) => {
      try { p.detach(); } catch {}
    });
    markerPluginsRef.current = [];
    seriesRefs.current.forEach((s) => {
      try { chartRef.current?.removeSeries(s); } catch {}
    });
    seriesRefs.current = [];
  }, []);

  // Render data
  useEffect(() => {
    const chart = chartRef.current;
    if (!chart) return;

    clearOverlays();

    const candleData: CandlestickData<Time>[] = klines
      .map((b) => {
        const t = typeof b.time === 'number' ? Math.floor(b.time / 1000) : msToSec(new Date(b.time).getTime());
        return {
          time: t as Time,
          open: b.open,
          high: b.high,
          low: b.low,
          close: b.close,
        };
      })
      .sort((a, b) => (a.time as number) - (b.time as number));

    const candleSeries = chart.addSeries(CandlestickSeries, {
      upColor: '#ef5350',
      downColor: '#26a69a',
      borderVisible: false,
      wickUpColor: '#ef5350',
      wickDownColor: '#26a69a',
    }, 0);
    candleSeries.setData(candleData);
    seriesRefs.current.push(candleSeries);
    candleSeriesRef.current = candleSeries;

    candleSeries.attachPrimitive(new PaneSeparator());

    // Render CalIndicator histogram (pane 1)
    const indicatorData = klines
      .map((b) => {
        const t = typeof b.time === 'number' ? Math.floor(b.time / 1000) : msToSec(new Date(b.time).getTime());
        return {
          time: t as Time,
          value: b.calIndicator ?? 0,
          color: (b.calIndicator ?? 0) > 0 ? '#ef5350' : '#26a69a',
        };
      })
      .filter((d) => d.value !== 0)
      .sort((a, b) => (a.time as number) - (b.time as number));

    if (indicatorData.length > 0) {
      const histSeries = chart.addSeries(HistogramSeries, {
        color: '#26a69a',
        base: 0,
        priceFormat: { type: 'price', precision: 2, minMove: 0.01 },
        lastValueVisible: false,
        priceLineVisible: false,
      }, 1);
      histSeries.setData(indicatorData);
      seriesRefs.current.push(histSeries);

      // 标题绘制在 pane 1 顶部，天然位于分界线下方
      histSeries.attachPrimitive(new PaneTitlePrimitive('指标计算结果'));

      const markers: SeriesMarker<Time>[] = indicatorData.map((d) => ({
        time: d.time,
        position: d.value > 0 ? 'aboveBar' : 'belowBar',
        shape: 'square',
        color: d.color ?? '#26a69a',
        text: String(Math.round(d.value * 100) / 100),
        size: 1,
      }));
      const plugin = createSeriesMarkers(histSeries, markers);
      markerPluginsRef.current.push(plugin);
    }

    // 十字光标 tooltip
    const crosshairHandler = (param: any) => {
      if (!param.time || !param.seriesData) {
        setTooltip(null);
        return;
      }
      const data = param.seriesData.get(candleSeries) as CandlestickData<Time> | undefined;
      if (!data || typeof data.open !== 'number') {
        setTooltip(null);
        return;
      }
      const change = data.close - data.open;
      const changePct = data.open !== 0 ? (change / data.open) * 100 : 0;
      const point = param.point ?? { x: 0, y: 0 };
      setTooltip({
        time: formatTime(param.time as number),
        open: data.open,
        high: data.high,
        low: data.low,
        close: data.close,
        change,
        changePct,
        x: point.x,
        y: point.y,
      });
    };
    chart.subscribeCrosshairMove(crosshairHandler);

    // 点击K线弹窗
    const clickHandler = (param: any) => {
      if (!param.time) return;
      const clickSec = param.time as number;
      // 找到对应的K线数据（允许前后1秒误差）
      const matched = klines.find((k) => {
        const kSec = typeof k.time === 'number' ? Math.floor(k.time / 1000) : msToSec(new Date(k.time).getTime());
        return Math.abs(kSec - clickSec) <= 1;
      });
      if (!matched) return;

      const point = param.point ?? { x: 0, y: 0 };
      const rect = containerRef.current?.getBoundingClientRect();
      const viewportX = rect ? rect.left + point.x : point.x;
      const viewportY = rect ? rect.top + point.y : point.y;
      setModal({
        time: formatDateTime(matched.time),
        open: matched.open,
        high: matched.high,
        low: matched.low,
        close: matched.close,
        volume: matched.volume,
        patterns: matched.patterns ?? [],
        patternDirection: matched.patternDirection ?? '',
        patternSignal: matched.patternSignal ?? '',
        x: viewportX,
        y: viewportY,
        // 量能关系指标
        volumeRatio5: matched.volumeRatio5,
        volumeChangePct: matched.volumeChangePct,
        obv: matched.obv,
        volumeSignal: matched.volumeSignal,
        volumeBullish: matched.volumeBullish,
        // 海龟交易法则指标
        turtleHigh20: matched.turtleHigh20,
        turtleHigh50: matched.turtleHigh50,
        turtleLow20: matched.turtleLow20,
        turtleLow50: matched.turtleLow50,
        turtleBreakoutHigh20: matched.turtleBreakoutHigh20,
        turtleBreakoutHigh50: matched.turtleBreakoutHigh50,
        turtleBreakdownLow20: matched.turtleBreakdownLow20,
        turtleBreakdownLow50: matched.turtleBreakdownLow50,
        turtleSignal: matched.turtleSignal,
        turtleBullish: matched.turtleBullish,
        // KDJ 指标基础值
        kdjK: matched.kdjK,
        kdjD: matched.kdjD,
        kdjJ: matched.kdjJ,
        // KDJ 指标信号
        kdjSignal: matched.kdjSignal,
        kdjBullish: matched.kdjBullish,
        kdjGoldenCross: matched.kdjGoldenCross,
        kdjDeathCross: matched.kdjDeathCross,
        kdjBottomDivergence: matched.kdjBottomDivergence,
        kdjTopDivergence: matched.kdjTopDivergence,
      });
    };
    chart.subscribeClick(clickHandler);

    chart.timeScale().fitContent();

    if (!chanlun) return;

    if (showBi && chanlun.biList) {
      chanlun.biList.forEach((bi) => {
        const series = chart.addSeries(LineSeries, {
          color: '#FFD700',
          lineWidth: 2,
          lastValueVisible: false,
          priceLineVisible: false,
          crosshairMarkerVisible: false,
        }, 0);
        series.setData([
          { time: msToSec(bi.startTime) as Time, value: bi.startPrice },
          { time: msToSec(bi.endTime) as Time, value: bi.endPrice },
        ]);
        seriesRefs.current.push(series);
      });
    }

    if (showSeg && chanlun.segList) {
      chanlun.segList.forEach((seg) => {
        const series = chart.addSeries(LineSeries, {
          color: '#FF0000',
          lineWidth: 3,
          lastValueVisible: false,
          priceLineVisible: false,
          crosshairMarkerVisible: false,
        }, 0);
        series.setData([
          { time: msToSec(seg.startTime) as Time, value: seg.startPrice },
          { time: msToSec(seg.endTime) as Time, value: seg.endPrice },
        ]);
        seriesRefs.current.push(series);
      });
    }

    const renderPivots = (pivotList: typeof chanlun.biPivotList, colorBase: string) => {
      if (!pivotList) return;
      pivotList.forEach((pivot) => {
        const startSec = msToSec(pivot.startTime);
        const endSec = msToSec(pivot.endTime);
        const borderColor = `${colorBase}0.9)`;
        const fillColor = `${colorBase}0.12)`;

        const fillSeries = chart.addSeries(BaselineSeries, {
          baseValue: { type: 'price', price: pivot.zd },
          topLineColor: 'transparent',
          topFillColor1: fillColor,
          topFillColor2: fillColor,
          bottomLineColor: 'transparent',
          bottomFillColor1: 'transparent',
          bottomFillColor2: 'transparent',
          lineWidth: 1,
          lastValueVisible: false,
          priceLineVisible: false,
          crosshairMarkerVisible: false,
        }, 0);
        fillSeries.setData([
          { time: startSec as Time, value: pivot.zg },
          { time: endSec as Time, value: pivot.zg },
        ]);
        seriesRefs.current.push(fillSeries);

        const topSeries = chart.addSeries(LineSeries, {
          color: borderColor,
          lineWidth: 1,
          lastValueVisible: false,
          priceLineVisible: false,
          crosshairMarkerVisible: false,
        }, 0);
        topSeries.setData([
          { time: startSec as Time, value: pivot.zg },
          { time: endSec as Time, value: pivot.zg },
        ]);
        seriesRefs.current.push(topSeries);

        const bottomSeries = chart.addSeries(LineSeries, {
          color: borderColor,
          lineWidth: 1,
          lastValueVisible: false,
          priceLineVisible: false,
          crosshairMarkerVisible: false,
        }, 0);
        bottomSeries.setData([
          { time: startSec as Time, value: pivot.zd },
          { time: endSec as Time, value: pivot.zd },
        ]);
        seriesRefs.current.push(bottomSeries);
      });
    };

    if (showBiPivot) renderPivots(chanlun.biPivotList, 'rgba(255, 215, 0, ');
    if (showSegPivot) renderPivots(chanlun.segPivotList, 'rgba(255, 0, 0, ');

    if (showMergedKLine && chanlun.mergedKLines) {
      chanlun.mergedKLines.forEach((kl) => {
        const startSec = msToSec(kl.startTime);
        const endSec = msToSec(kl.endTime);
        if (startSec === endSec) return;

        const color =
          kl.direction === 'up'
            ? 'rgba(38, 166, 154, 0.25)'
            : kl.direction === 'down'
            ? 'rgba(239, 83, 80, 0.25)'
            : 'rgba(150, 150, 150, 0.15)';

        const series = chart.addSeries(BaselineSeries, {
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
        }, 0);
        series.setData([
          { time: startSec as Time, value: kl.high },
          { time: endSec as Time, value: kl.high },
        ]);
        seriesRefs.current.push(series);
      });
    }

    chart.timeScale().fitContent();

    return () => {
      chart.unsubscribeCrosshairMove(crosshairHandler);
      chart.unsubscribeClick(clickHandler);
    };
  }, [klines, chanlun, showBi, showSeg, showBiPivot, showSegPivot, showMergedKLine, clearOverlays]);

  const isUp = tooltip ? tooltip.close >= tooltip.open : false;

  return (
    <div style={{ width: '100%', height: '100%', position: 'relative' }}>
      {/* K线信息浮层 —— 跟随鼠标 */}
      <div
        style={{
          position: 'absolute',
          left: tooltip
            ? Math.max(8, Math.min(tooltip.x + 16, (containerRef.current?.clientWidth ?? 400) - 196))
            : 0,
          top: tooltip
            ? Math.max(8, tooltip.y - 8)
            : 0,
          zIndex: 10,
          background: 'rgba(19, 23, 34, 0.92)',
          border: '1px solid #2a2e39',
          borderRadius: 6,
          padding: '10px 14px',
          fontSize: 12,
          color: '#d1d4dc',
          pointerEvents: 'none',
          minWidth: 180,
          display: tooltip ? 'block' : 'none',
          boxShadow: '0 4px 12px rgba(0,0,0,0.4)',
        }}
      >
        {tooltip && (
          <>
            <div style={{ fontSize: 11, color: '#868993', marginBottom: 6 }}>{tooltip.time}</div>
            <div style={{ display: 'grid', gridTemplateColumns: '40px 1fr', gap: '4px 8px', lineHeight: '1.6' }}>
              <span style={{ color: '#868993' }}>开盘</span>
              <span style={{ textAlign: 'right', fontFamily: 'Consolas, monospace' }}>{tooltip.open.toFixed(2)}</span>
              <span style={{ color: '#868993' }}>最高</span>
              <span style={{ textAlign: 'right', fontFamily: 'Consolas, monospace', color: '#26a69a' }}>{tooltip.high.toFixed(2)}</span>
              <span style={{ color: '#868993' }}>最低</span>
              <span style={{ textAlign: 'right', fontFamily: 'Consolas, monospace', color: '#ef5350' }}>{tooltip.low.toFixed(2)}</span>
              <span style={{ color: '#868993' }}>收盘</span>
              <span style={{ textAlign: 'right', fontFamily: 'Consolas, monospace', fontWeight: 600, color: isUp ? '#ef5350' : '#26a69a' }}>
                {tooltip.close.toFixed(2)}
              </span>
              <span style={{ color: '#868993' }}>涨跌</span>
              <span style={{ textAlign: 'right', fontFamily: 'Consolas, monospace', color: tooltip.change >= 0 ? '#ef5350' : '#26a69a' }}>
                {tooltip.change >= 0 ? '+' : ''}{tooltip.change.toFixed(2)} ({tooltip.changePct >= 0 ? '+' : ''}{tooltip.changePct.toFixed(2)}%)
              </span>
            </div>
          </>
        )}
      </div>

      {/* 蜡烛图形态弹窗 —— 跟随点击位置 */}
      {modal && (
        <>
          {/* 透明遮罩：点击空白处关闭 */}
          <div
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              bottom: 0,
              zIndex: 99,
            }}
            onClick={() => setModal(null)}
          />
          <div
            style={{
              position: 'fixed',
              left: Math.max(8, Math.min(modal.x - 200, window.innerWidth - 408)),
              top: Math.max(40, modal.y - 20),
              bottom: 8,
              zIndex: 100,
              background: '#1e222d',
              border: '1px solid #2a2e39',
              borderRadius: 8,
              padding: '16px 20px',
              width: 400,
              overflowY: 'auto',
              boxShadow: '0 8px 32px rgba(0,0,0,0.6)',
              pointerEvents: 'auto',
            }}
          >
            {/* 标题栏 */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <h3 style={{ margin: 0, fontSize: 15, color: '#fff', fontWeight: 600 }}>📊 K线详情</h3>
              <button
                onClick={() => setModal(null)}
                style={{
                  background: 'transparent',
                  border: 'none',
                  color: '#868993',
                  fontSize: 20,
                  cursor: 'pointer',
                  lineHeight: 1,
                }}
              >
                ×
              </button>
            </div>

            {/* K线基础信息 */}
            <div style={{ fontSize: 12, color: '#868993', marginBottom: 12 }}>{modal.time}</div>
            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr 1fr 1fr',
              gap: 8,
              marginBottom: 16,
              padding: '10px 12px',
              background: '#131722',
              borderRadius: 6,
            }}>
              <div>
                <div style={{ fontSize: 10, color: '#868993' }}>开盘</div>
                <div style={{ fontSize: 13, fontWeight: 600, color: '#d1d4dc' }}>{modal.open.toFixed(2)}</div>
              </div>
              <div>
                <div style={{ fontSize: 10, color: '#868993' }}>最高</div>
                <div style={{ fontSize: 13, fontWeight: 600, color: '#26a69a' }}>{modal.high.toFixed(2)}</div>
              </div>
              <div>
                <div style={{ fontSize: 10, color: '#868993' }}>最低</div>
                <div style={{ fontSize: 13, fontWeight: 600, color: '#ef5350' }}>{modal.low.toFixed(2)}</div>
              </div>
              <div>
                <div style={{ fontSize: 10, color: '#868993' }}>收盘</div>
                <div style={{ fontSize: 13, fontWeight: 600, color: modal.close >= modal.open ? '#ef5350' : '#26a69a' }}>
                  {modal.close.toFixed(2)}
                </div>
              </div>
            </div>

            {/* 蜡烛图形态 */}
            <div style={{ marginBottom: 12 }}>
              <div style={{ fontSize: 11, color: '#868993', marginBottom: 8, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                蜡烛图形态
              </div>
              {modal.patterns.length > 0 ? (
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
                  {modal.patterns.map((name, idx) => (
                    <span
                      key={idx}
                      style={{
                        fontSize: 12,
                        padding: '3px 10px',
                        borderRadius: 12,
                        background: modal.patternDirection === 'Bullish' ? 'rgba(239, 83, 80, 0.2)' :
                                    modal.patternDirection === 'Bearish' ? 'rgba(38, 166, 154, 0.2)' :
                                    'rgba(150, 150, 150, 0.15)',
                        color: modal.patternDirection === 'Bullish' ? '#ef5350' :
                               modal.patternDirection === 'Bearish' ? '#26a69a' :
                               '#d1d4dc',
                        border: `1px solid ${modal.patternDirection === 'Bullish' ? 'rgba(239, 83, 80, 0.4)' :
                                              modal.patternDirection === 'Bearish' ? 'rgba(38, 166, 154, 0.4)' :
                                              'rgba(150, 150, 150, 0.3)'}`,
                      }}
                    >
                      {name}
                    </span>
                  ))}
                </div>
              ) : (
                <div style={{ fontSize: 12, color: '#5a5e69' }}>无特殊形态</div>
              )}
            </div>

            {/* 信号判断 */}
            {modal.patternSignal && (
              <div style={{
                padding: '10px 12px',
                borderRadius: 6,
                background: modal.patternDirection === 'Bullish' ? 'rgba(239, 83, 80, 0.1)' :
                            modal.patternDirection === 'Bearish' ? 'rgba(38, 166, 154, 0.1)' :
                            'rgba(150, 150, 150, 0.08)',
                borderLeft: `3px solid ${modal.patternDirection === 'Bullish' ? '#ef5350' :
                                          modal.patternDirection === 'Bearish' ? '#26a69a' :
                                          '#868993'}`,
              }}>
                <div style={{ fontSize: 12, fontWeight: 600, color: modal.patternDirection === 'Bullish' ? '#ef5350' :
                                                                       modal.patternDirection === 'Bearish' ? '#26a69a' :
                                                                       '#d1d4dc' }}>
                  {modal.patternSignal}
                </div>
              </div>
            )}

            {/* 量能关系指标 */}
            <div style={{ marginTop: 12 }}>
              <div style={{ fontSize: 11, color: '#868993', marginBottom: 8, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                量能分析
              </div>
              <div style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr 1fr',
                gap: 8,
                marginBottom: 10,
                padding: '8px 10px',
                background: '#131722',
                borderRadius: 6,
              }}>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>5日量比</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: '#d1d4dc' }}>
                    {modal.volumeRatio5 != null ? modal.volumeRatio5.toFixed(2) : '-'}
                  </div>
                </div>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>量增减(%)</div>
                  <div style={{
                    fontSize: 12, fontWeight: 600,
                    color: (modal.volumeChangePct ?? 0) >= 0 ? '#ef5350' : '#26a69a'
                  }}>
                    {modal.volumeChangePct != null
                      ? `${modal.volumeChangePct >= 0 ? '+' : ''}${modal.volumeChangePct.toFixed(2)}%`
                      : '-'}
                  </div>
                </div>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>OBV</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: '#d1d4dc' }}>
                    {modal.obv != null ? Math.round(modal.obv).toLocaleString() : '-'}
                  </div>
                </div>
              </div>

              {modal.volumeSignal && (
                <div style={{
                  padding: '10px 12px',
                  borderRadius: 6,
                  background: modal.volumeBullish === true ? 'rgba(239, 83, 80, 0.1)' :
                              modal.volumeBullish === false ? 'rgba(38, 166, 154, 0.1)' :
                              'rgba(150, 150, 150, 0.08)',
                  borderLeft: `3px solid ${modal.volumeBullish === true ? '#ef5350' :
                                            modal.volumeBullish === false ? '#26a69a' :
                                            '#868993'}`,
                }}>
                  <div style={{
                    fontSize: 12, fontWeight: 600,
                    color: modal.volumeBullish === true ? '#ef5350' :
                           modal.volumeBullish === false ? '#26a69a' :
                           '#d1d4dc'
                  }}>
                    {modal.volumeBullish === true && '【看涨】 '}
                    {modal.volumeBullish === false && '【看跌】 '}
                    {modal.volumeBullish === null && '【观望】 '}
                    {modal.volumeSignal}
                  </div>
                </div>
              )}
            </div>

            {/* 海龟交易法则指标 */}
            <div style={{ marginTop: 12 }}>
              <div style={{ fontSize: 11, color: '#868993', marginBottom: 8, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                海龟交易法则
              </div>
              <div style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr 1fr 1fr',
                gap: 8,
                marginBottom: 10,
                padding: '8px 10px',
                background: '#131722',
                borderRadius: 6,
              }}>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>20日最高</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: '#d1d4dc' }}>
                    {modal.turtleHigh20 != null ? modal.turtleHigh20.toFixed(2) : '-'}
                  </div>
                </div>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>20日最低</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: '#d1d4dc' }}>
                    {modal.turtleLow20 != null ? modal.turtleLow20.toFixed(2) : '-'}
                  </div>
                </div>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>50日最高</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: '#d1d4dc' }}>
                    {modal.turtleHigh50 != null ? modal.turtleHigh50.toFixed(2) : '-'}
                  </div>
                </div>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>50日最低</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: '#d1d4dc' }}>
                    {modal.turtleLow50 != null ? modal.turtleLow50.toFixed(2) : '-'}
                  </div>
                </div>
              </div>

              {/* 突破/跌破标识 */}
              <div style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr 1fr 1fr',
                gap: 8,
                marginBottom: 10,
              }}>
                <div style={{ textAlign: 'center', padding: '4px 0', borderRadius: 4, background: modal.turtleBreakoutHigh20 ? 'rgba(239,83,80,0.15)' : 'transparent' }}>
                  <div style={{ fontSize: 10, color: '#868993' }}>突破20日高</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: modal.turtleBreakoutHigh20 ? '#ef5350' : '#5a5e69' }}>
                    {modal.turtleBreakoutHigh20 ? '✓ 突破' : '—'}
                  </div>
                </div>
                <div style={{ textAlign: 'center', padding: '4px 0', borderRadius: 4, background: modal.turtleBreakoutHigh50 ? 'rgba(239,83,80,0.15)' : 'transparent' }}>
                  <div style={{ fontSize: 10, color: '#868993' }}>突破50日高</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: modal.turtleBreakoutHigh50 ? '#ef5350' : '#5a5e69' }}>
                    {modal.turtleBreakoutHigh50 ? '✓ 突破' : '—'}
                  </div>
                </div>
                <div style={{ textAlign: 'center', padding: '4px 0', borderRadius: 4, background: modal.turtleBreakdownLow20 ? 'rgba(38,166,154,0.15)' : 'transparent' }}>
                  <div style={{ fontSize: 10, color: '#868993' }}>跌破20日低</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: modal.turtleBreakdownLow20 ? '#26a69a' : '#5a5e69' }}>
                    {modal.turtleBreakdownLow20 ? '✓ 跌破' : '—'}
                  </div>
                </div>
                <div style={{ textAlign: 'center', padding: '4px 0', borderRadius: 4, background: modal.turtleBreakdownLow50 ? 'rgba(38,166,154,0.15)' : 'transparent' }}>
                  <div style={{ fontSize: 10, color: '#868993' }}>跌破50日低</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: modal.turtleBreakdownLow50 ? '#26a69a' : '#5a5e69' }}>
                    {modal.turtleBreakdownLow50 ? '✓ 跌破' : '—'}
                  </div>
                </div>
              </div>

              {modal.turtleSignal && (
                <div style={{
                  padding: '10px 12px',
                  borderRadius: 6,
                  background: modal.turtleBullish === true ? 'rgba(239, 83, 80, 0.1)' :
                              modal.turtleBullish === false ? 'rgba(38, 166, 154, 0.1)' :
                              'rgba(150, 150, 150, 0.08)',
                  borderLeft: `3px solid ${modal.turtleBullish === true ? '#ef5350' :
                                            modal.turtleBullish === false ? '#26a69a' :
                                            '#868993'}`,
                }}>
                  <div style={{
                    fontSize: 12, fontWeight: 600,
                    color: modal.turtleBullish === true ? '#ef5350' :
                           modal.turtleBullish === false ? '#26a69a' :
                           '#d1d4dc'
                  }}>
                    {modal.turtleBullish === true && '【看涨】 '}
                    {modal.turtleBullish === false && '【看跌】 '}
                    {modal.turtleBullish === null && '【观望】 '}
                    {modal.turtleSignal}
                  </div>
                </div>
              )}
            </div>

            {/* KDJ 指标信号 */}
            <div style={{ marginTop: 12 }}>
              <div style={{ fontSize: 11, color: '#868993', marginBottom: 8, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                KDJ 随机指标
              </div>
              <div style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr 1fr',
                gap: 8,
                marginBottom: 10,
                padding: '8px 10px',
                background: '#131722',
                borderRadius: 6,
              }}>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>K 值</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: '#d1d4dc' }}>
                    {modal.kdjK != null ? modal.kdjK.toFixed(2) : '-'}
                  </div>
                </div>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>D 值</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: '#d1d4dc' }}>
                    {modal.kdjD != null ? modal.kdjD.toFixed(2) : '-'}
                  </div>
                </div>
                <div>
                  <div style={{ fontSize: 10, color: '#868993' }}>J 值</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: (modal.kdjJ ?? 0) > 100 ? '#ef5350' : (modal.kdjJ ?? 0) < 0 ? '#26a69a' : '#d1d4dc' }}>
                    {modal.kdjJ != null ? modal.kdjJ.toFixed(2) : '-'}
                  </div>
                </div>
              </div>
              <div style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr 1fr 1fr',
                gap: 8,
                marginBottom: 10,
              }}>
                <div style={{ textAlign: 'center', padding: '4px 0', borderRadius: 4, background: modal.kdjGoldenCross ? 'rgba(239,83,80,0.15)' : 'transparent' }}>
                  <div style={{ fontSize: 10, color: '#868993' }}>金叉</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: modal.kdjGoldenCross ? '#ef5350' : '#5a5e69' }}>
                    {modal.kdjGoldenCross ? '✓ 金叉' : '—'}
                  </div>
                </div>
                <div style={{ textAlign: 'center', padding: '4px 0', borderRadius: 4, background: modal.kdjDeathCross ? 'rgba(38,166,154,0.15)' : 'transparent' }}>
                  <div style={{ fontSize: 10, color: '#868993' }}>死叉</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: modal.kdjDeathCross ? '#26a69a' : '#5a5e69' }}>
                    {modal.kdjDeathCross ? '✓ 死叉' : '—'}
                  </div>
                </div>
                <div style={{ textAlign: 'center', padding: '4px 0', borderRadius: 4, background: modal.kdjBottomDivergence ? 'rgba(239,83,80,0.15)' : 'transparent' }}>
                  <div style={{ fontSize: 10, color: '#868993' }}>底背离</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: modal.kdjBottomDivergence ? '#ef5350' : '#5a5e69' }}>
                    {modal.kdjBottomDivergence ? '✓ 背离' : '—'}
                  </div>
                </div>
                <div style={{ textAlign: 'center', padding: '4px 0', borderRadius: 4, background: modal.kdjTopDivergence ? 'rgba(38,166,154,0.15)' : 'transparent' }}>
                  <div style={{ fontSize: 10, color: '#868993' }}>顶背离</div>
                  <div style={{ fontSize: 12, fontWeight: 600, color: modal.kdjTopDivergence ? '#26a69a' : '#5a5e69' }}>
                    {modal.kdjTopDivergence ? '✓ 背离' : '—'}
                  </div>
                </div>
              </div>

              {modal.kdjSignal && (
                <div style={{
                  padding: '10px 12px',
                  borderRadius: 6,
                  background: modal.kdjBullish === true ? 'rgba(239, 83, 80, 0.1)' :
                              modal.kdjBullish === false ? 'rgba(38, 166, 154, 0.1)' :
                              'rgba(150, 150, 150, 0.08)',
                  borderLeft: `3px solid ${modal.kdjBullish === true ? '#ef5350' :
                                            modal.kdjBullish === false ? '#26a69a' :
                                            '#868993'}`,
                }}>
                  <div style={{
                    fontSize: 12, fontWeight: 600,
                    color: modal.kdjBullish === true ? '#ef5350' :
                           modal.kdjBullish === false ? '#26a69a' :
                           '#d1d4dc'
                  }}>
                    {modal.kdjBullish === true && '【看涨】 '}
                    {modal.kdjBullish === false && '【看跌】 '}
                    {modal.kdjBullish === null && '【观望】 '}
                    {modal.kdjSignal}
                  </div>
                </div>
              )}
            </div>
          </div>
        </>
      )}

      {/* 下窗口标题：由 PaneTitlePrimitive 在 pane 1 内部绘制，天然跟随分界线 */}

      <div
        ref={containerRef}
        style={{
          width: '100%',
          height: '100%',
          minHeight: '500px',
          position: 'relative',
        }}
      />
    </div>
  );
}
