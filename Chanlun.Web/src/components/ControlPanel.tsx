import { useState, useCallback } from 'react';

export type Resolution = 'Minute1' | 'Minute5' | 'Minute15' | 'Minute30' | 'Minute60' | 'Day' | 'Week' | 'Month';

interface Props {
  onCalculate: (symbol: string, resolution: Resolution, fromDate: string, toDate: string) => void;
  onTdxCalculate: () => void;
  loading: boolean;
  apiUrl: string;
  onApiUrlChange: (url: string) => void;
  resolution: Resolution;
  onResolutionChange: (r: Resolution) => void;
}

export default function ControlPanel({ onCalculate, onTdxCalculate, loading, apiUrl, onApiUrlChange, resolution, onResolutionChange }: Props) {
  const [symbol, setSymbol] = useState('000066');

  const now = new Date();
  const oneYearAgo = new Date(now.getFullYear() - 1, now.getMonth(), now.getDate());
  const [startDate, setStartDate] = useState(oneYearAgo.toISOString().split('T')[0]);
  const [endDate, setEndDate] = useState(now.toISOString().split('T')[0]);
  const [showBi, setShowBi] = useState(true);
  const [showSeg, setShowSeg] = useState(true);
  const [showBiPivot, setShowBiPivot] = useState(true);
  const [showSegPivot, setShowSegPivot] = useState(true);
  const [showMergedKLine, setShowMergedKLine] = useState(true);

  const resolutionMap: Record<Resolution, string> = {
    'Minute1': '1分钟',
    'Minute5': '5分钟',
    'Minute15': '15分钟',
    'Minute30': '30分钟',
    'Minute60': '1小时',
    'Day': '日线',
    'Week': '周线',
    'Month': '月线',
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
    </div>
  );
}
