import { useState, useCallback } from 'react';
import ChanLunChart from './components/ChanLunChart';
import ControlPanel, { type Resolution } from './components/ControlPanel';
import { calculateChanlun, setBaseUrl, getBaseUrl } from './api/chanlunApi';
import type { ChanlunResponse, KlineBar } from './types/chanlun';

function App() {
  const [klines, setKlines] = useState<KlineBar[]>([]);
  const [chanlun, setChanlun] = useState<ChanlunResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [apiUrl, setApiUrl] = useState(getBaseUrl());
  const [error, setError] = useState<string>('');
  const [resolution, setResolution] = useState<Resolution>('D');

  // Display options
  const [showBi, setShowBi] = useState(true);
  const [showSeg, setShowSeg] = useState(true);
  const [showBiPivot, setShowBiPivot] = useState(true);
  const [showSegPivot, setShowSegPivot] = useState(true);
  const [showMergedKLine, setShowMergedKLine] = useState(false);

  const handleApiUrlChange = useCallback((url: string) => {
    setApiUrl(url);
    setBaseUrl(url);
  }, []);

  const resolutionLabel: Record<Resolution, string> = {
    '1': '1分钟', '5': '5分钟', '15': '15分钟', '30': '30分钟',
    '60': '1小时', '240': '4小时', 'D': '日线', 'W': '周线', 'M': '月线',
  };

  const handleCalculate = useCallback(async (symbol: string, bars: KlineBar[], _resolution: Resolution) => {
    setLoading(true);
    setError('');
    try {
      const data = await calculateChanlun(symbol, bars);
      setKlines(bars);
      setChanlun(data);
    } catch (err: any) {
      setError(err?.response?.data?.error || err.message || '计算失败');
      setChanlun(null);
    } finally {
      setLoading(false);
    }
  }, []);

  const handleClear = useCallback(() => {
    setKlines([]);
    setChanlun(null);
    setError('');
  }, []);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', width: '100vw', height: '100vh', background: '#131722', color: '#d1d4dc' }}>
      {/* Header */}
      <div style={{ background: '#1e222d', padding: '10px 20px', borderBottom: '1px solid #2a2e39', display: 'flex', alignItems: 'center', gap: '12px', flexWrap: 'wrap' }}>
        <h1 style={{ fontSize: '16px', color: '#fff', marginRight: 'auto' }}>📈 ChanLun 缠论图表</h1>
        {error && (
          <span style={{ fontSize: '12px', padding: '4px 10px', borderRadius: '4px', background: 'rgba(231, 76, 60, 0.15)', color: '#e74c3c' }}>
            ❌ {error}
          </span>
        )}
        {chanlun && !error && (
          <span style={{ fontSize: '12px', padding: '4px 10px', borderRadius: '4px', background: 'rgba(46, 204, 113, 0.15)', color: '#2ecc71' }}>
            ✅ [{resolutionLabel[resolution]}] 笔:{chanlun.biList?.length || 0} 线段:{chanlun.segList?.length || 0} 笔中枢:{chanlun.biPivotList?.length || 0} 线段中枢:{chanlun.segPivotList?.length || 0}
          </span>
        )}
      </div>

      {/* Main */}
      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        <ControlPanel
          onCalculate={handleCalculate}
          onClear={handleClear}
          loading={loading}
          apiUrl={apiUrl}
          onApiUrlChange={handleApiUrlChange}
          resolution={resolution}
          onResolutionChange={setResolution}
        />
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
          {/* Toolbar */}
          <div style={{ padding: '8px 16px', borderBottom: '1px solid #2a2e39', display: 'flex', gap: '12px', alignItems: 'center' }}>
            <span style={{ fontSize: '12px', color: '#868993' }}>图层控制:</span>
            <label style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '12px', cursor: 'pointer' }}>
              <input type="checkbox" checked={showBi} onChange={(e) => setShowBi(e.target.checked)} />
              笔
            </label>
            <label style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '12px', cursor: 'pointer' }}>
              <input type="checkbox" checked={showSeg} onChange={(e) => setShowSeg(e.target.checked)} />
              线段
            </label>
            <label style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '12px', cursor: 'pointer' }}>
              <input type="checkbox" checked={showBiPivot} onChange={(e) => setShowBiPivot(e.target.checked)} />
              笔中枢
            </label>
            <label style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '12px', cursor: 'pointer' }}>
              <input type="checkbox" checked={showSegPivot} onChange={(e) => setShowSegPivot(e.target.checked)} />
              线段中枢
            </label>
            <label style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '12px', cursor: 'pointer' }}>
              <input type="checkbox" checked={showMergedKLine} onChange={(e) => setShowMergedKLine(e.target.checked)} />
              合并K线
            </label>
          </div>
          <div style={{ flex: 1, position: 'relative' }}>
            <ChanLunChart
              klines={klines}
              chanlun={chanlun}
              showBi={showBi}
              showSeg={showSeg}
              showBiPivot={showBiPivot}
              showSegPivot={showSegPivot}
              showMergedKLine={showMergedKLine}
            />
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;
