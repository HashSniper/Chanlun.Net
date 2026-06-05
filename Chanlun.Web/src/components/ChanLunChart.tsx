import { useEffect, useRef, useState } from 'react';
import {
  createChart,
  CandlestickSeries,
  LineSeries,
  BaselineSeries,
  type IChartApi,
  type ISeriesApi,
  type CandlestickData,
  type Time,
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

function parseTimeToSec(time: string | number): number {
  if (typeof time === 'number') return Math.floor(time / 1000);
  // yyyyMMdd format
  if (/^\d{8}$/.test(time)) {
    const y = parseInt(time.slice(0, 4), 10);
    const m = parseInt(time.slice(4, 6), 10) - 1;
    const d = parseInt(time.slice(6, 8), 10);
    return Math.floor(new Date(y, m, d).getTime() / 1000);
  }
  // ISO string fallback
  return Math.floor(new Date(time).getTime() / 1000);
}

function formatTime(sec: number): string {
  const d = new Date(sec * 1000);
  return d.toLocaleString('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
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
  const candleSeriesRef = useRef<ISeriesApi<'Candlestick'> | null>(null);
  const [tooltip, setTooltip] = useState<TooltipData | null>(null);

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

    const resizeObserver = new ResizeObserver(() => {
      if (containerRef.current) {
        chart.applyOptions({ width: containerRef.current.clientWidth, height: containerRef.current.clientHeight });
      }
    });
    resizeObserver.observe(containerRef.current);

    return () => {
      resizeObserver.disconnect();
      chart.unsubscribeCrosshairMove(() => {});
      chart.remove();
      chartRef.current = null;
      candleSeriesRef.current = null;
    };
  }, []);

  // Clear overlays
  const clearOverlays = () => {
    seriesRefs.current.forEach((s) => {
      try { chartRef.current?.removeSeries(s); } catch {}
    });
    seriesRefs.current = [];
  };

  // Render data
  useEffect(() => {
    const chart = chartRef.current;
    if (!chart) return;

    clearOverlays();

    // Render K-lines
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
      upColor: '#26a69a',
      downColor: '#ef5350',
      borderVisible: false,
      wickUpColor: '#26a69a',
      wickDownColor: '#ef5350',
    });
    candleSeries.setData(candleData);
    seriesRefs.current.push(candleSeries);
    candleSeriesRef.current = candleSeries;

    // 监听十字光标移动，显示K线详细信息
    chart.subscribeCrosshairMove((param) => {
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
      setTooltip({
        time: formatTime(param.time as number),
        open: data.open,
        high: data.high,
        low: data.low,
        close: data.close,
        change,
        changePct,
      });
    });

    chart.timeScale().fitContent();

    if (!chanlun) return;

    // Render Bi
    if (showBi && chanlun.biList) {
      chanlun.biList.forEach((bi) => {
        const color = bi.direction === 'up' ? '#ff6d00' : '#00c853';
        const series = chart.addSeries(LineSeries, {
          color,
          lineWidth: 2,
          lastValueVisible: false,
          priceLineVisible: false,
          crosshairMarkerVisible: false,
        });
        series.setData([
          { time: msToSec(bi.startTime) as Time, value: bi.startPrice },
          { time: msToSec(bi.endTime) as Time, value: bi.endPrice },
        ]);
        seriesRefs.current.push(series);
      });
    }

    // Render Seg
    if (showSeg && chanlun.segList) {
      chanlun.segList.forEach((seg) => {
        const color = seg.direction === 'up' ? '#d50000' : '#00bfa5';
        const series = chart.addSeries(LineSeries, {
          color,
          lineWidth: 3,
          lastValueVisible: false,
          priceLineVisible: false,
          crosshairMarkerVisible: false,
        });
        series.setData([
          { time: msToSec(seg.startTime) as Time, value: seg.startPrice },
          { time: msToSec(seg.endTime) as Time, value: seg.endPrice },
        ]);
        seriesRefs.current.push(series);
      });
    }

    // Render Pivots helper
    const renderPivots = (pivotList: typeof chanlun.biPivotList, colorBase: string, alpha: number) => {
      if (!pivotList) return;
      pivotList.forEach((pivot) => {
        const color = `${colorBase}${alpha})`;
        const fill1 = `${colorBase}${Math.min(alpha * 1.2, 0.6)})`;
        const fill2 = `${colorBase}0.05)`;

        const series = chart.addSeries(BaselineSeries, {
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
          { time: msToSec(pivot.startTime) as Time, value: pivot.zg },
          { time: msToSec(pivot.endTime) as Time, value: pivot.zg },
        ]);
        seriesRefs.current.push(series);
      });
    };

    if (showBiPivot) renderPivots(chanlun.biPivotList, 'rgba(41, 98, 255, ', 0.35);
    if (showSegPivot) renderPivots(chanlun.segPivotList, 'rgba(255, 171, 0, ', 0.35);

    // Render merged K-lines
    if (showMergedKLine && chanlun.mergedKLines) {
      chanlun.mergedKLines.forEach((kl) => {
        const startSec = msToSec(kl.startTime);
        const endSec = msToSec(kl.endTime);
        // 跳过时间相同的单根K线合并（无法绘制区域）
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
        });
        series.setData([
          { time: startSec as Time, value: kl.high },
          { time: endSec as Time, value: kl.high },
        ]);
        seriesRefs.current.push(series);
      });
    }

    chart.timeScale().fitContent();
  }, [klines, chanlun, showBi, showSeg, showBiPivot, showSegPivot, showMergedKLine]);

  const isUp = tooltip ? tooltip.close >= tooltip.open : false;

  return (
    <div style={{ width: '100%', height: '100%', position: 'relative' }}>
      {/* K线信息浮层 */}
      <div
        style={{
          position: 'absolute',
          top: 10,
          right: 10,
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
              <span style={{ textAlign: 'right', fontFamily: 'Consolas, monospace', fontWeight: 600, color: isUp ? '#26a69a' : '#ef5350' }}>
                {tooltip.close.toFixed(2)}
              </span>
              <span style={{ color: '#868993' }}>涨跌</span>
              <span style={{ textAlign: 'right', fontFamily: 'Consolas, monospace', color: tooltip.change >= 0 ? '#26a69a' : '#ef5350' }}>
                {tooltip.change >= 0 ? '+' : ''}{tooltip.change.toFixed(2)} ({tooltip.changePct >= 0 ? '+' : ''}{tooltip.changePct.toFixed(2)}%)
              </span>
            </div>
          </>
        )}
      </div>

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
