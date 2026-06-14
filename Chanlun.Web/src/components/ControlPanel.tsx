import { useState, useCallback, useEffect } from 'react';
import TradingPanel from './TradingPanel';

export type Resolution = 'Minute1' | 'Minute5' | 'Minute15' | 'Minute30' | 'Minute60' | 'Day';

interface Props {
  onCalculate: (symbol: string, resolution: Resolution, fromDate: string, toDate: string) => void;
  onTdxCalculate: () => void;
  loading: boolean;
  apiUrl: string;
  onApiUrlChange: (url: string) => void;
  resolution: Resolution;
  onResolutionChange: (r: Resolution) => void;
  currentSymbol?: string;
  currentResolution?: Resolution;
  currentFromTime?: string;
  currentToTime?: string;
}

export default function ControlPanel({ onCalculate, onTdxCalculate, loading, apiUrl, onApiUrlChange, resolution, onResolutionChange, currentSymbol, currentResolution, currentFromTime, currentToTime }: Props) {
  const [symbol, setSymbol] = useState('000066');
  const [showTradingPanel, setShowTradingPanel] = useState(false);

  // API 返回后同步股票代码和周期到左侧显示
  useEffect(() => {
    if (currentSymbol) {
      setSymbol(currentSymbol);
    }
  }, [currentSymbol]);

  useEffect(() => {
    if (currentResolution) {
      onResolutionChange(currentResolution);
    }
  }, [currentResolution, onResolutionChange]);

  useEffect(() => {
    if (currentFromTime) {
      setStartDate(currentFromTime.split('T')[0]);
    }
  }, [currentFromTime]);

  useEffect(() => {
    if (currentToTime) {
      setEndDate(currentToTime.split('T')[0]);
    }
  }, [currentToTime]);

  const now = new Date();
  const oneYearAgo = new Date(now.getFullYear() - 1, now.getMonth(), now.getDate());
  const [startDate, setStartDate] = useState(oneYearAgo.toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(now.toISOString().split('T')[0]);

  const resolutionMap: Record<Resolution, string> = {
    'Minute1': '1分钟',
    'Minute5': '5分钟',
    'Minute15': '15分钟',
    'Minute30': '30分钟',
    'Minute60': '1小时',
    'Day': '日线',
  };

  const handleFetchAndCalculate = useCallback(async () => {
    const sym = symbol.trim().toUpperCase();
    if (!sym) {
      alert('请输入股票代码');
      return;
    }
    onCalculate(sym, resolution, startDate, endDate);
  }, [symbol, resolution, startDate, endDate, onCalculate]);

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
          onClick={handleFetchAndCalculate}
          disabled={loading}
          style={{ width: '100%', padding: '8px 0', background: '#2962FF', color: '#fff', border: 'none', borderRadius: '4px', fontSize: '13px', fontWeight: 500, cursor: loading ? 'not-allowed' : 'pointer', opacity: loading ? 0.7 : 1, marginBottom: '8px' }}
        >
          {loading ? '计算中...' : '📡 获取 K线并计算缠论'}
        </button>

        <button
          onClick={onTdxCalculate}
          disabled={loading}
          style={{ width: '100%', padding: '8px 0', background: '#7B1FA2', color: '#fff', border: 'none', borderRadius: '4px', fontSize: '13px', fontWeight: 500, cursor: loading ? 'not-allowed' : 'pointer', opacity: loading ? 0.7 : 1, marginBottom: '8px' }}
        >
          {loading ? '计算中...' : '📈 同步通达信当前窗口数据'}
        </button>

        <div style={{ fontSize: '11px', color: '#868993', lineHeight: 1.5, marginTop: '6px' }}>
          💡 <b>使用说明：</b><br/>
          1. 输入股票代码 & 选择时间范围<br/>
          2. 点击「获取 K线并计算缠论」加载数据<br/>
          3. 或点击「获取通达信缠论数据」加载当前盯盘数据
        </div>
      </div>

<div style={{ borderTop: '1px solid #2a2e39', paddingTop: '12px' }}>
        <button
          onClick={() => setShowTradingPanel(true)}
          style={{ width: '100%', padding: '10px 0', background: '#2a2e39', color: '#d1d4dc', border: '1px solid #3a3e49', borderRadius: '4px', fontSize: '13px', fontWeight: 500, cursor: 'pointer', marginBottom: '12px' }}
        >
          💼 交易管理
        </button>
        <h3 style={{ fontSize: '13px', marginBottom: '8px', color: '#fff', fontWeight: 600 }}>📐 图例</h3>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '3px', borderRadius: '2px', background: '#FFD700' }}></div>
          笔
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '3px', borderRadius: '2px', background: '#FF0000' }}></div>
          线段
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '5px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '8px', borderRadius: '2px', background: 'rgba(255,215,0,0.6)' }}></div>
          笔中枢
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '11px', color: '#868993' }}>
          <div style={{ width: '16px', height: '8px', borderRadius: '2px', background: 'rgba(255,0,0,0.6)' }}></div>
          线段中枢
        </div>
      </div>

      {/* 交易管理弹窗 */}
      {showTradingPanel && (
        <>
          <div
            style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, zIndex: 99, background: 'rgba(0,0,0,0.5)' }}
            onClick={() => setShowTradingPanel(false)}
          />
          <div style={{ position: 'fixed', top: '50%', left: '50%', transform: 'translate(-50%, -50%)', zIndex: 100, width: 700, maxWidth: '90vw', height: 600, maxHeight: '85vh', background: '#1e222d', borderRadius: 8, border: '1px solid #2a2e39', boxShadow: '0 8px 32px rgba(0,0,0,0.6)', display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
            <div style={{ padding: '12px 16px', borderBottom: '1px solid #2a2e39', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontSize: 15, fontWeight: 600, color: '#fff' }}>💼 交易管理</span>
              <button onClick={() => setShowTradingPanel(false)} style={{ background: 'transparent', border: 'none', color: '#868993', fontSize: 20, cursor: 'pointer', lineHeight: 1 }}>×</button>
            </div>
            <div style={{ flex: 1, overflow: 'hidden' }}>
              <TradingPanel symbol={currentSymbol || symbol} />
            </div>
          </div>
        </>
      )}
    </div>
  );
}
