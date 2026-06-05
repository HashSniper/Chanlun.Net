import { useState, useCallback } from 'react';
import type { KlineBar } from '../types/chanlun';
import { getUdfHistory, pushKlines } from '../api/chanlunApi';

export type Resolution = '1' | '5' | '15' | '30' | '60' | '240' | 'D' | 'W' | 'M';

interface Props {
  onCalculate: (symbol: string, bars: KlineBar[], resolution: Resolution) => void;
  onClear: () => void;
  loading: boolean;
  apiUrl: string;
  onApiUrlChange: (url: string) => void;
  resolution: Resolution;
  onResolutionChange: (r: Resolution) => void;
}

export default function ControlPanel({ onCalculate, onClear, loading, apiUrl, onApiUrlChange, resolution, onResolutionChange }: Props) {
  const [symbol, setSymbol] = useState('SH600000');
  const [jsonText, setJsonText] = useState('');
  const [fetching, setFetching] = useState(false);
  const [pushing, setPushing] = useState(false);
  const [lastFetchMsg, setLastFetchMsg] = useState('');
  const now = new Date();
  const oneYearAgo = new Date(now.getFullYear() - 1, now.getMonth(), now.getDate());
  const [startDate, setStartDate] = useState(oneYearAgo.toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(now.toISOString().split('T')[0]);
  const [showBi, setShowBi] = useState(true);
  const [showSeg, setShowSeg] = useState(true);
  const [showBiPivot, setShowBiPivot] = useState(true);
  const [showSegPivot, setShowSegPivot] = useState(true);
  const [showMergedKLine, setShowMergedKLine] = useState(false);

  const resolutionMap: Record<Resolution, string> = {
    '1': '1分钟',
    '5': '5分钟',
    '15': '15分钟',
    '30': '30分钟',
    '60': '1小时',
    '240': '4小时',
    'D': '日线',
    'W': '周线',
    'M': '月线',
  };

  const handleCalculate = useCallback(() => {
    if (!jsonText.trim()) {
      alert('请输入 K 线 JSON 数据，或点击「从后端获取」加载数据');
      return;
    }
    let bars: KlineBar[] = [];
    try {
      bars = JSON.parse(jsonText);
    } catch {
      alert('JSON 格式错误');
      return;
    }
    if (!Array.isArray(bars) || bars.length === 0) {
      alert('K 线数据为空');
      return;
    }
    onCalculate(symbol.trim().toUpperCase(), bars, resolution);
  }, [symbol, jsonText, resolution, onCalculate]);

  const handleFetchFromApi = useCallback(async () => {
    const sym = symbol.trim().toUpperCase();
    if (!sym) {
      alert('请输入股票代码');
      return;
    }
    setFetching(true);
    setLastFetchMsg('');
    try {
      const from = Math.floor(new Date(startDate + 'T00:00:00Z').getTime() / 1000);
      const to = Math.floor(new Date(endDate + 'T23:59:59Z').getTime() / 1000);
      const data = await getUdfHistory(sym, resolution, from, to);
      if (data.s === 'no_data') {
        setLastFetchMsg('后端暂无该股票数据，请先点击「推送示例数据到后端」');
        setFetching(false);
        return;
      }
      if (data.s === 'error') {
        setLastFetchMsg(`获取失败: ${data.errmsg || '未知错误'}`);
        setFetching(false);
        return;
      }
      if (!data.t || data.t.length === 0) {
        setLastFetchMsg('返回数据为空');
        setFetching(false);
        return;
      }
      const bars: KlineBar[] = data.t.map((t, i) => ({
        time: new Date(t * 1000).toISOString(),
        open: data.o![i],
        high: data.h![i],
        low: data.l![i],
        close: data.c![i],
        volume: data.v ? data.v[i] : 0,
      }));
      setJsonText(JSON.stringify(bars, null, 2));
      setLastFetchMsg(`✅ 成功获取 ${bars.length} 根K线`);
    } catch (err: any) {
      setLastFetchMsg(`❌ 请求失败: ${err.message || '网络错误'}`);
    } finally {
      setFetching(false);
    }
  }, [symbol, resolution]);

  const handlePushSample = useCallback(async () => {
    const sym = symbol.trim().toUpperCase();
    if (!sym) {
      alert('请输入股票代码');
      return;
    }
    setPushing(true);
    setLastFetchMsg('');
    try {
      const sample: KlineBar[] = [
        { time: '2024-01-01T09:30:00', open: 10.0, high: 10.0, low: 10.0, close: 10.0, volume: 1000 },
        { time: '2024-01-02T09:30:00', open: 10.0, high: 10.5, low: 9.8, close: 10.2, volume: 1200 },
        { time: '2024-01-03T09:30:00', open: 10.2, high: 10.8, low: 10.0, close: 10.6, volume: 1500 },
        { time: '2024-01-04T09:30:00', open: 10.6, high: 10.9, low: 10.3, close: 10.4, volume: 1100 },
        { time: '2024-01-05T09:30:00', open: 10.4, high: 10.7, low: 10.1, close: 10.3, volume: 1300 },
        { time: '2024-01-08T09:30:00', open: 10.3, high: 10.6, low: 9.9, close: 10.0, volume: 1400 },
        { time: '2024-01-09T09:30:00', open: 10.0, high: 10.2, low: 9.5, close: 9.7, volume: 1600 },
        { time: '2024-01-10T09:30:00', open: 9.7, high: 9.9, low: 9.3, close: 9.4, volume: 1800 },
        { time: '2024-01-11T09:30:00', open: 9.4, high: 9.8, low: 9.2, close: 9.6, volume: 1700 },
        { time: '2024-01-12T09:30:00', open: 9.6, high: 10.0, low: 9.5, close: 9.9, volume: 1500 },
        { time: '2024-01-15T09:30:00', open: 9.9, high: 10.3, low: 9.7, close: 10.1, volume: 1400 },
        { time: '2024-01-16T09:30:00', open: 10.1, high: 10.5, low: 9.9, close: 10.4, volume: 1300 },
        { time: '2024-01-17T09:30:00', open: 10.4, high: 10.8, low: 10.2, close: 10.7, volume: 1200 },
        { time: '2024-01-18T09:30:00', open: 10.7, high: 11.0, low: 10.5, close: 10.9, volume: 1100 },
        { time: '2024-01-19T09:30:00', open: 10.9, high: 11.2, low: 10.7, close: 11.0, volume: 1000 },
        { time: '2024-01-22T09:30:00', open: 11.0, high: 11.3, low: 10.8, close: 11.1, volume: 1100 },
        { time: '2024-01-23T09:30:00', open: 11.1, high: 11.5, low: 10.9, close: 11.3, volume: 1200 },
        { time: '2024-01-24T09:30:00', open: 11.3, high: 11.6, low: 11.0, close: 11.2, volume: 1300 },
        { time: '2024-01-25T09:30:00', open: 11.2, high: 11.4, low: 10.8, close: 10.9, volume: 1400 },
        { time: '2024-01-26T09:30:00', open: 10.9, high: 11.0, low: 10.5, close: 10.6, volume: 1500 },
        { time: '2024-01-29T09:30:00', open: 10.6, high: 10.8, low: 10.2, close: 10.3, volume: 1600 },
        { time: '2024-01-30T09:30:00', open: 10.3, high: 10.5, low: 9.9, close: 10.0, volume: 1700 },
        { time: '2024-01-31T09:30:00', open: 10.0, high: 10.2, low: 9.6, close: 9.7, volume: 1800 },
        { time: '2024-02-01T09:30:00', open: 9.7, high: 10.0, low: 9.5, close: 9.8, volume: 1600 },
        { time: '2024-02-02T09:30:00', open: 9.8, high: 10.3, low: 9.7, close: 10.1, volume: 1400 },
        { time: '2024-02-05T09:30:00', open: 10.1, high: 10.6, low: 10.0, close: 10.4, volume: 1300 },
        { time: '2024-02-06T09:30:00', open: 10.4, high: 10.9, low: 10.3, close: 10.7, volume: 1200 },
        { time: '2024-02-07T09:30:00', open: 10.7, high: 11.1, low: 10.5, close: 10.8, volume: 1100 },
        { time: '2024-02-08T09:30:00', open: 10.8, high: 11.2, low: 10.6, close: 11.0, volume: 1000 },
        { time: '2024-02-19T09:30:00', open: 11.0, high: 11.4, low: 10.8, close: 11.2, volume: 1100 },
        { time: '2024-02-20T09:30:00', open: 11.2, high: 11.5, low: 11.0, close: 11.1, volume: 1200 },
        { time: '2024-02-21T09:30:00', open: 11.1, high: 11.3, low: 10.7, close: 10.8, volume: 1300 },
        { time: '2024-02-22T09:30:00', open: 10.8, high: 11.0, low: 10.4, close: 10.5, volume: 1400 },
        { time: '2024-02-23T09:30:00', open: 10.5, high: 10.7, low: 10.1, close: 10.2, volume: 1500 },
        { time: '2024-02-26T09:30:00', open: 10.2, high: 10.4, low: 9.8, close: 9.9, volume: 1600 },
        { time: '2024-02-27T09:30:00', open: 9.9, high: 10.1, low: 9.5, close: 9.6, volume: 1700 },
        { time: '2024-02-28T09:30:00', open: 9.6, high: 9.8, low: 9.2, close: 9.3, volume: 1800 },
        { time: '2024-02-29T09:30:00', open: 9.3, high: 9.7, low: 9.1, close: 9.5, volume: 1600 },
        { time: '2024-03-01T09:30:00', open: 9.5, high: 10.0, low: 9.4, close: 9.8, volume: 1400 },
        { time: '2024-03-04T09:30:00', open: 9.8, high: 10.3, low: 9.7, close: 10.1, volume: 1300 },
        { time: '2024-03-05T09:30:00', open: 10.1, high: 10.5, low: 9.9, close: 10.3, volume: 1200 },
        { time: '2024-03-06T09:30:00', open: 10.3, high: 10.7, low: 10.1, close: 10.5, volume: 1100 },
        { time: '2024-03-07T09:30:00', open: 10.5, high: 10.9, low: 10.3, close: 10.8, volume: 1000 },
        { time: '2024-03-08T09:30:00', open: 10.8, high: 11.2, low: 10.6, close: 11.0, volume: 1100 },
        { time: '2024-03-11T09:30:00', open: 11.0, high: 11.4, low: 10.8, close: 11.1, volume: 1200 },
        { time: '2024-03-12T09:30:00', open: 11.1, high: 11.3, low: 10.7, close: 10.9, volume: 1300 },
        { time: '2024-03-13T09:30:00', open: 10.9, high: 11.0, low: 10.5, close: 10.6, volume: 1400 },
        { time: '2024-03-14T09:30:00', open: 10.6, high: 10.8, low: 10.2, close: 10.3, volume: 1500 },
        { time: '2024-03-15T09:30:00', open: 10.3, high: 10.5, low: 9.9, close: 10.0, volume: 1600 },
      ];
      await pushKlines(sym, sample);
      setJsonText(JSON.stringify(sample, null, 2));
      setLastFetchMsg(`✅ 已推送 ${sample.length} 根示例K线到后端，现在可以点击「从后端获取」或「计算缠论」`);
    } catch (err: any) {
      setLastFetchMsg(`❌ 推送失败: ${err.message || '网络错误'}`);
    } finally {
      setPushing(false);
    }
  }, [symbol]);

  return (
    <div style={{ width: '300px', minWidth: '280px', background: '#1e222d', borderRight: '1px solid #2a2e39', padding: '16px', overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '14px' }}>
      <div>
        <h3 style={{ fontSize: '13px', marginBottom: '10px', color: '#fff', fontWeight: 600 }}>🔌 API 设置</h3>
        <input
          type="text"
          value={apiUrl}
          onChange={(e) => onApiUrlChange(e.target.value)}
          placeholder="API地址"
          style={{ width: '100%', padding: '6px 10px', fontSize: '12px', background: '#131722', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: '4px', outline: 'none' }}
        />
      </div>

      <div>
        <h3 style={{ fontSize: '13px', marginBottom: '10px', color: '#fff', fontWeight: 600 }}>📊 股票代码 & 周期</h3>
        <input
          type="text"
          value={symbol}
          onChange={(e) => setSymbol(e.target.value)}
          placeholder="股票代码"
          style={{ width: '100%', padding: '6px 10px', fontSize: '12px', background: '#131722', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: '4px', outline: 'none', marginBottom: '8px' }}
        />
        <div style={{ display: 'flex', gap: '8px', marginBottom: '8px' }}>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: '10px', color: '#868993', marginBottom: '3px' }}>开始日期</div>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              style={{ width: '100%', padding: '5px 8px', fontSize: '12px', background: '#131722', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: '4px', outline: 'none' }}
            />
          </div>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: '10px', color: '#868993', marginBottom: '3px' }}>结束日期</div>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              style={{ width: '100%', padding: '5px 8px', fontSize: '12px', background: '#131722', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: '4px', outline: 'none' }}
            />
          </div>
        </div>
        <select
          value={resolution}
          onChange={(e) => onResolutionChange(e.target.value as Resolution)}
          style={{ width: '100%', padding: '6px 10px', fontSize: '12px', background: '#131722', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: '4px', outline: 'none' }}
        >
          {(Object.keys(resolutionMap) as Resolution[]).map(r => (
            <option key={r} value={r}>{resolutionMap[r]}</option>
          ))}
        </select>
      </div>

      <div style={{ borderTop: '1px solid #2a2e39', paddingTop: '12px' }}>
        <h3 style={{ fontSize: '13px', marginBottom: '10px', color: '#fff', fontWeight: 600 }}>📥 数据获取</h3>

        <button
          onClick={handleFetchFromApi}
          disabled={fetching}
          style={{ width: '100%', padding: '8px 0', background: '#2e7d32', color: '#fff', border: 'none', borderRadius: '4px', fontSize: '13px', fontWeight: 500, cursor: fetching ? 'not-allowed' : 'pointer', opacity: fetching ? 0.7 : 1, marginBottom: '8px' }}
        >
          {fetching ? '获取中...' : '📡 从后端获取 K线'}
        </button>

        <button
          onClick={handlePushSample}
          disabled={pushing}
          style={{ width: '100%', padding: '8px 0', background: '#1565c0', color: '#fff', border: 'none', borderRadius: '4px', fontSize: '13px', fontWeight: 500, cursor: pushing ? 'not-allowed' : 'pointer', opacity: pushing ? 0.7 : 1, marginBottom: '8px' }}
        >
          {pushing ? '推送中...' : '📤 推送示例数据到后端'}
        </button>

        {lastFetchMsg && (
          <div style={{ fontSize: '11px', padding: '6px 8px', borderRadius: '4px', background: lastFetchMsg.startsWith('✅') ? 'rgba(46, 204, 113, 0.12)' : 'rgba(231, 76, 60, 0.12)', color: lastFetchMsg.startsWith('✅') ? '#2ecc71' : '#e74c3c', lineHeight: 1.4 }}>
            {lastFetchMsg}
          </div>
        )}

        <div style={{ fontSize: '11px', color: '#868993', lineHeight: 1.5, marginTop: '6px' }}>
          💡 <b>使用说明：</b><br/>
          1. 输入股票代码 & 选择时间范围<br/>
          2. 点击「推送示例数据到后端」<br/>
          3. 再点击「从后端获取 K线」<br/>
          4. 最后点击「计算缠论」
        </div>
      </div>

      <div>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
          <h3 style={{ fontSize: '13px', color: '#fff', fontWeight: 600 }}>📋 K线数据 (JSON)</h3>
          <button
            onClick={handlePushSample}
            style={{ padding: '3px 10px', fontSize: '11px', background: '#444', color: '#d1d4dc', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
          >
            加载示例
          </button>
        </div>
        <textarea
          value={jsonText}
          onChange={(e) => setJsonText(e.target.value)}
          placeholder='[{&quot;time&quot;:&quot;2024-01-01T09:30:00&quot;,&quot;open&quot;:10,&quot;high&quot;:10.5,&quot;low&quot;:9.8,&quot;close&quot;:10.2,&quot;volume&quot;:1000}, ...]'
          style={{ width: '100%', height: '140px', background: '#131722', border: '1px solid #2a2e39', color: '#d1d4dc', padding: '10px', fontFamily: 'Consolas, Monaco, monospace', fontSize: '11px', borderRadius: '4px', resize: 'vertical', outline: 'none' }}
        />
      </div>
      
      <div style={{ display: 'flex', gap: '8px' }}>
        <button
          onClick={handleCalculate}
          disabled={loading}
          style={{ flex: 1, padding: '10px 0', background: '#2962FF', color: '#fff', border: 'none', borderRadius: '4px', fontSize: '14px', fontWeight: 600, cursor: loading ? 'not-allowed' : 'pointer', opacity: loading ? 0.7 : 1 }}
        >
          {loading ? '计算中...' : '🧮 计算缠论'}
        </button>
        <button
          onClick={onClear}
          style={{ padding: '10px 16px', background: '#444', color: '#d1d4dc', border: 'none', borderRadius: '4px', fontSize: '13px', cursor: 'pointer' }}
        >
          清除
        </button>
      </div>

      <div style={{ borderTop: '1px solid #2a2e39', paddingTop: '12px' }}>
        <h3 style={{ fontSize: '13px', marginBottom: '10px', color: '#fff', fontWeight: 600 }}>⚙️ 显示选项</h3>
        <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '12px', marginBottom: '6px', cursor: 'pointer' }}>
          <input type="checkbox" checked={showBi} onChange={(e) => setShowBi(e.target.checked)} />
          显示笔
        </label>
        <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '12px', marginBottom: '6px', cursor: 'pointer' }}>
          <input type="checkbox" checked={showSeg} onChange={(e) => setShowSeg(e.target.checked)} />
          显示线段
        </label>
        <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '12px', marginBottom: '6px', cursor: 'pointer' }}>
          <input type="checkbox" checked={showBiPivot} onChange={(e) => setShowBiPivot(e.target.checked)} />
          显示笔中枢
        </label>
        <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '12px', marginBottom: '6px', cursor: 'pointer' }}>
          <input type="checkbox" checked={showSegPivot} onChange={(e) => setShowSegPivot(e.target.checked)} />
          显示线段中枢
        </label>
        <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '12px', cursor: 'pointer' }}>
          <input type="checkbox" checked={showMergedKLine} onChange={(e) => setShowMergedKLine(e.target.checked)} />
          显示合并K线
        </label>
      </div>

      <div style={{ borderTop: '1px solid #2a2e39', paddingTop: '12px' }}>
        <h3 style={{ fontSize: '13px', marginBottom: '8px', color: '#fff', fontWeight: 600 }}>📐 图例</h3>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '3px', borderRadius: '2px', background: '#ff6d00' }}></div>
          向上笔
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '3px', borderRadius: '2px', background: '#00c853' }}></div>
          向下笔
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '3px', borderRadius: '2px', background: '#d50000' }}></div>
          向上线段
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '3px', borderRadius: '2px', background: '#00bfa5' }}></div>
          向下线段
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '8px', borderRadius: '2px', background: 'rgba(41,98,255,0.6)' }}></div>
          笔中枢
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '8px', borderRadius: '2px', background: 'rgba(255,171,0,0.6)' }}></div>
          线段中枢
        </div>
      </div>
    </div>
  );
}
