import { useState, useEffect, useMemo } from 'react';
import type { TradingAccount, StockTradeRecord } from '../types/trading';
import { getAccounts, getAllRecords } from '../api/tradingApi';

export default function TradeReportPage() {
  const [accounts, setAccounts] = useState<TradingAccount[]>([]);
  const [records, setRecords] = useState<StockTradeRecord[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // 筛选条件
  const [accountFilter, setAccountFilter] = useState<number | ''>('');
  const [symbolFilter, setSymbolFilter] = useState('');
  const [directionFilter, setDirectionFilter] = useState<'All' | 'Buy' | 'Sell'>('All');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError('');
    try {
      const [acctList, allRecords] = await Promise.all([
        getAccounts(),
        getAllRecords(10000),
      ]);
      setAccounts(acctList);
      setRecords(allRecords);
    } catch (err: any) {
      setError(err?.response?.data?.error || '加载数据失败');
    } finally {
      setLoading(false);
    }
  };

  const accountMap = useMemo(() => {
    const map: Record<number, string> = {};
    accounts.forEach(a => { map[a.id] = a.name; });
    return map;
  }, [accounts]);

  const filteredRecords = useMemo(() => {
    return records.filter(r => {
      if (accountFilter !== '' && r.accountId !== accountFilter) return false;
      if (symbolFilter && !r.symbol.toLowerCase().includes(symbolFilter.toLowerCase())) return false;
      if (directionFilter !== 'All' && r.direction !== directionFilter) return false;
      if (dateFrom && new Date(r.tradeTime) < new Date(dateFrom)) return false;
      if (dateTo) {
        const to = new Date(dateTo);
        to.setDate(to.getDate() + 1);
        if (new Date(r.tradeTime) >= to) return false;
      }
      return true;
    });
  }, [records, accountFilter, symbolFilter, directionFilter, dateFrom, dateTo]);

  const stats = useMemo(() => {
    let buyCount = 0, sellCount = 0;
    let buyAmount = 0, sellAmount = 0;
    let totalFee = 0, totalTax = 0;
    filteredRecords.forEach(r => {
      totalFee += r.fee;
      totalTax += r.tax;
      if (r.direction === 'Buy') {
        buyCount++;
        buyAmount += r.amount;
      } else {
        sellCount++;
        sellAmount += r.amount;
      }
    });
    return {
      totalCount: filteredRecords.length,
      buyCount,
      sellCount,
      buyAmount,
      sellAmount,
      totalFee,
      totalTax,
      netAmount: sellAmount - buyAmount,
    };
  }, [filteredRecords]);

  const formatMoney = (v: number) => v.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  const formatDate = (s: string) => new Date(s).toLocaleString('zh-CN', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });

  return (
    <div style={{ width: '100%', height: '100%', background: '#131722', color: '#d1d4dc', display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
      {/* Header */}
      <div style={{ padding: '14px 20px', borderBottom: '1px solid #2a2e39', display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 10 }}>
        <h2 style={{ fontSize: 16, color: '#fff', margin: 0 }}>📜 交易记录报表</h2>
        <button
          onClick={loadData}
          disabled={loading}
          style={{ padding: '6px 14px', fontSize: 12, background: '#2962FF', color: '#fff', border: 'none', borderRadius: 4, cursor: loading ? 'not-allowed' : 'pointer', opacity: loading ? 0.7 : 1 }}
        >
          {loading ? '加载中...' : '🔄 刷新'}
        </button>
      </div>

      {error && (
        <div style={{ padding: '8px 20px', background: 'rgba(231, 76, 60, 0.15)', color: '#e74c3c', fontSize: 12 }}>
          {error}
        </div>
      )}

      {/* 筛选栏 */}
      <div style={{ padding: '12px 20px', borderBottom: '1px solid #2a2e39', display: 'flex', gap: 10, flexWrap: 'wrap', alignItems: 'center' }}>
        <select
          value={accountFilter}
          onChange={(e) => setAccountFilter(e.target.value === '' ? '' : Number(e.target.value))}
          style={{ padding: '5px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4, minWidth: 140 }}
        >
          <option value="">全部账户</option>
          {accounts.map(a => (
            <option key={a.id} value={a.id}>{a.name}</option>
          ))}
        </select>

        <input
          type="text"
          value={symbolFilter}
          onChange={(e) => setSymbolFilter(e.target.value)}
          placeholder="股票代码"
          style={{ padding: '5px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4, width: 100 }}
        />

        <select
          value={directionFilter}
          onChange={(e) => setDirectionFilter(e.target.value as 'All' | 'Buy' | 'Sell')}
          style={{ padding: '5px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }}
        >
          <option value="All">全部方向</option>
          <option value="Buy">买入</option>
          <option value="Sell">卖出</option>
        </select>

        <input
          type="date"
          value={dateFrom}
          onChange={(e) => setDateFrom(e.target.value)}
          style={{ padding: '5px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }}
        />
        <span style={{ fontSize: 12, color: '#868993' }}>至</span>
        <input
          type="date"
          value={dateTo}
          onChange={(e) => setDateTo(e.target.value)}
          style={{ padding: '5px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }}
        />
      </div>

      {/* 汇总统计 */}
      <div style={{ padding: '12px 20px', borderBottom: '1px solid #2a2e39', display: 'flex', gap: 20, flexWrap: 'wrap' }}>
        <StatCard label="总笔数" value={`${stats.totalCount}`} />
        <StatCard label="买入笔数" value={`${stats.buyCount}`} color="#ef5350" />
        <StatCard label="卖出笔数" value={`${stats.sellCount}`} color="#26a69a" />
        <StatCard label="买入金额" value={formatMoney(stats.buyAmount)} color="#ef5350" />
        <StatCard label="卖出金额" value={formatMoney(stats.sellAmount)} color="#26a69a" />
        <StatCard label="净额" value={`${stats.netAmount >= 0 ? '+' : ''}${formatMoney(stats.netAmount)}`} color={stats.netAmount >= 0 ? '#ef5350' : '#26a69a'} />
        <StatCard label="总手续费" value={formatMoney(stats.totalFee)} />
        <StatCard label="总印花税" value={formatMoney(stats.totalTax)} />
      </div>

      {/* 数据表格 */}
      <div style={{ flex: 1, overflow: 'auto', padding: '0 20px 20px' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #2a2e39', position: 'sticky', top: 0, background: '#131722' }}>
              <th style={{ textAlign: 'left', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>时间</th>
              <th style={{ textAlign: 'left', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>账户</th>
              <th style={{ textAlign: 'left', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>代码</th>
              <th style={{ textAlign: 'center', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>方向</th>
              <th style={{ textAlign: 'right', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>价格</th>
              <th style={{ textAlign: 'right', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>数量</th>
              <th style={{ textAlign: 'right', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>成交金额</th>
              <th style={{ textAlign: 'right', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>手续费</th>
              <th style={{ textAlign: 'right', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>印花税</th>
              <th style={{ textAlign: 'right', padding: '10px 8px', color: '#868993', fontWeight: 600 }}>发生金额</th>
            </tr>
          </thead>
          <tbody>
            {filteredRecords.length === 0 ? (
              <tr>
                <td colSpan={10} style={{ textAlign: 'center', padding: 40, color: '#5a5e69' }}>
                  暂无交易记录
                </td>
              </tr>
            ) : (
              filteredRecords.map(r => (
                <tr key={r.id} style={{ borderBottom: '1px solid #1e222d' }}>
                  <td style={{ padding: '8px', color: '#d1d4dc' }}>{formatDate(r.tradeTime)}</td>
                  <td style={{ padding: '8px', color: '#d1d4dc' }}>{accountMap[r.accountId] || `账户#${r.accountId}`}</td>
                  <td style={{ padding: '8px', color: '#d1d4dc', fontWeight: 600 }}>{r.symbol}</td>
                  <td style={{ padding: '8px', textAlign: 'center' }}>
                    <span style={{ color: r.direction === 'Buy' ? '#ef5350' : '#26a69a', fontWeight: 600 }}>
                      {r.direction === 'Buy' ? '买入' : '卖出'}
                    </span>
                  </td>
                  <td style={{ padding: '8px', textAlign: 'right', color: '#d1d4dc' }}>{r.price.toFixed(2)}</td>
                  <td style={{ padding: '8px', textAlign: 'right', color: '#d1d4dc' }}>{r.quantity}</td>
                  <td style={{ padding: '8px', textAlign: 'right', color: '#d1d4dc' }}>{formatMoney(r.amount)}</td>
                  <td style={{ padding: '8px', textAlign: 'right', color: '#868993' }}>{formatMoney(r.fee)}</td>
                  <td style={{ padding: '8px', textAlign: 'right', color: '#868993' }}>{formatMoney(r.tax)}</td>
                  <td style={{ padding: '8px', textAlign: 'right', color: '#d1d4dc', fontWeight: 600 }}>
                    {formatMoney(r.totalAmount)}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function StatCard({ label, value, color = '#d1d4dc' }: { label: string; value: string; color?: string }) {
  return (
    <div style={{ minWidth: 100 }}>
      <div style={{ fontSize: 10, color: '#868993', marginBottom: 2 }}>{label}</div>
      <div style={{ fontSize: 14, fontWeight: 600, color }}>{value}</div>
    </div>
  );
}
