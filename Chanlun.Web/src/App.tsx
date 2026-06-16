import { useState, useCallback, useEffect, useRef } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import ChanLunChart from './components/ChanLunChart';
import ControlPanel, { type Resolution } from './components/ControlPanel';
import TradeReportPage from './pages/TradeReportPage';

import { getChanlunKlines, getTdxChanlunKlines, setBaseUrl, getBaseUrl } from './api/chanlunApi';
import { startSignalRConnection, onTdxDataUpdated, stopSignalRConnection } from './api/signalrService';
import type { ChanlunResponse, KlineBar } from './types/chanlun';

function App() {
  const [currentView, setCurrentView] = useState<'chart' | 'tradeReport'>('chart');
  const [klines, setKlines] = useState<KlineBar[]>([]);
  const [chanlun, setChanlun] = useState<ChanlunResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [apiUrl, setApiUrl] = useState(getBaseUrl());
  const [error, setError] = useState<string>('');
  const [resolution, setResolution] = useState<Resolution>('Day');
  const [signalRStatus, setSignalRStatus] = useState<'connecting' | 'connected' | 'disconnected'>('connecting');
  const signalRStartedRef = useRef(false);
  const signalRConnRef = useRef<HubConnection | null>(null);

  // Display options
  const [showBi, setShowBi] = useState(true);
  const [showSeg, setShowSeg] = useState(true);
  const [showBiPivot, setShowBiPivot] = useState(true);
  const [showSegPivot, setShowSegPivot] = useState(true);
  const [showMergedKLine, setShowMergedKLine] = useState(true);

  const handleApiUrlChange = useCallback((url: string) => {
    setApiUrl(url);
    setBaseUrl(url);
  }, []);

  const resolutionLabel: Record<Resolution, string> = {
    'Minute1': '1分钟', 'Minute5': '5分钟', 'Minute15': '15分钟', 'Minute30': '30分钟',
    'Minute60': '1小时', 'Day': '日线',
  };

  const handleCalculate = useCallback(async (symbol: string, _resolution: Resolution, fromDate: string, toDate: string) => {
    setLoading(true);
    setError('');
    try {
      const data = await getChanlunKlines(symbol, _resolution, fromDate, toDate);
      setKlines(data.bars);
      setChanlun(data);
    } catch (err: any) {
      setError(err?.response?.data?.error || err.message || '请求失败');
      setChanlun(null);
      setKlines([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const handleTdxCalculate = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getTdxChanlunKlines();
      setKlines(data.bars);
      setChanlun(data);
    } catch (err: any) {
      setError(err?.response?.data?.error || err.message || '通达信数据请求失败');
      setChanlun(null);
      setKlines([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // 连接 SignalR，接收后端推送的通达信数据更新通知
  useEffect(() => {
    // 防止 StrictMode 下重复连接
    if (signalRStartedRef.current) return;
    signalRStartedRef.current = true;

    let unsubscribe: (() => void) | null = null;

    setSignalRStatus('connecting');
    startSignalRConnection()
      .then((conn) => {
        signalRConnRef.current = conn;
        setSignalRStatus('connected');
        unsubscribe = onTdxDataUpdated(conn, () => {
          handleTdxCalculate();
        });
      })
      .catch((err) => {
        signalRStartedRef.current = false;
        setSignalRStatus('disconnected');
        console.error('SignalR 连接失败:', err);
      });

    return () => {
      unsubscribe?.();
      const conn = signalRConnRef.current;
      if (conn) {
        stopSignalRConnection(conn).catch((err) => {
          console.error('SignalR 停止失败:', err);
        });
        signalRConnRef.current = null;
      }
      signalRStartedRef.current = false;
    };
  }, [handleTdxCalculate]);



  return (
    <div style={{ display: 'flex', flexDirection: 'column', width: '100vw', height: '100vh', background: '#131722', color: '#d1d4dc' }}>
      {/* Header */}
      <div style={{ background: '#1e222d', padding: '10px 20px', borderBottom: '1px solid #2a2e39', display: 'flex', alignItems: 'center', gap: '12px', flexWrap: 'wrap' }}>
        <h1 style={{ fontSize: '16px', color: '#fff', marginRight: 'auto' }}>📈 ChanLun 缠论图表</h1>
        <div style={{ display: 'flex', gap: 4 }}>
          <button
            onClick={() => setCurrentView('chart')}
            style={{
              padding: '5px 12px', fontSize: 12, borderRadius: 4, border: 'none',
              cursor: 'pointer',
              background: currentView === 'chart' ? '#2962FF' : '#2a2e39',
              color: currentView === 'chart' ? '#fff' : '#868993',
            }}
          >
            📊 图表
          </button>
          <button
            onClick={() => setCurrentView('tradeReport')}
            style={{
              padding: '5px 12px', fontSize: 12, borderRadius: 4, border: 'none',
              cursor: 'pointer',
              background: currentView === 'tradeReport' ? '#2962FF' : '#2a2e39',
              color: currentView === 'tradeReport' ? '#fff' : '#868993',
            }}
          >
            📜 交易报表
          </button>
        </div>
        <span style={{
          fontSize: '12px', padding: '4px 10px', borderRadius: '4px',
          background: signalRStatus === 'connected' ? 'rgba(46, 204, 113, 0.15)' : signalRStatus === 'connecting' ? 'rgba(241, 196, 15, 0.15)' : 'rgba(231, 76, 60, 0.15)',
          color: signalRStatus === 'connected' ? '#2ecc71' : signalRStatus === 'connecting' ? '#f1c40f' : '#e74c3c',
        }}>
          {signalRStatus === 'connected' ? '🟢 SignalR 已连接' : signalRStatus === 'connecting' ? '🟡 SignalR 连接中' : '🔴 SignalR 未连接'}
        </span>
        {error && (
          <span style={{ fontSize: '12px', padding: '4px 10px', borderRadius: '4px', background: 'rgba(231, 76, 60, 0.15)', color: '#e74c3c' }}>
            ❌ {error}
          </span>
        )}
        {currentView === 'chart' && chanlun && !error && (
          <span style={{ fontSize: '12px', padding: '4px 10px', borderRadius: '4px', background: 'rgba(46, 204, 113, 0.15)', color: '#2ecc71' }}>
            ✅ {chanlun.symbol} [{resolutionLabel[chanlun.resolution as Resolution] ?? chanlun.resolution}] K线:{chanlun.barCount} 笔:{chanlun.biList?.length || 0} 线段:{chanlun.segList?.length || 0} 笔中枢:{chanlun.biPivotList?.length || 0} 线段中枢:{chanlun.segPivotList?.length || 0}
          </span>
        )}
      </div>

      {/* Main */}
      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        {currentView === 'tradeReport' ? (
          <TradeReportPage />
        ) : (
        <>
        <ControlPanel
          onCalculate={handleCalculate}
          onTdxCalculate={handleTdxCalculate}
          loading={loading}
          apiUrl={apiUrl}
          onApiUrlChange={handleApiUrlChange}
          resolution={resolution}
          onResolutionChange={setResolution}
          currentSymbol={chanlun?.symbol}
          currentResolution={chanlun?.resolution as Resolution | undefined}
          currentFromTime={chanlun?.fromTime}
          currentToTime={chanlun?.toTime}
        />
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
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
        </>
        )}
      </div>
    </div>
  );
}

export default App;
