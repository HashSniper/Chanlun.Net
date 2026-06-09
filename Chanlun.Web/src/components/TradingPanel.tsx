import { useState, useEffect, useCallback } from 'react';
import type { TradingAccount, StockPosition } from '../types/trading';
import {
  getAccounts, createAccount, deleteAccount, getPositions, executeTrade, settlement
} from '../api/tradingApi';

interface Props {
  symbol?: string;
}

export default function TradingPanel({ symbol }: Props) {
  const [accounts, setAccounts] = useState<TradingAccount[]>([]);
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(null);
  const [positions, setPositions] = useState<StockPosition[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // 创建账户弹窗
  const [showCreateAccount, setShowCreateAccount] = useState(false);
  const [newAccountName, setNewAccountName] = useState('');
  const [newAccountBalance, setNewAccountBalance] = useState('100000');

  // 下单弹窗
  const [showOrderForm, setShowOrderForm] = useState(false);
  const [orderDirection, setOrderDirection] = useState<'Buy' | 'Sell'>('Buy');
  const [orderPrice, setOrderPrice] = useState('');
  const [orderQuantity, setOrderQuantity] = useState('100');
  const [orderFee, setOrderFee] = useState('5');
  const [orderTax, setOrderTax] = useState('0');
  const [orderSymbol, setOrderSymbol] = useState(symbol || '');

  // 刷新账户列表
  const refreshAccounts = useCallback(async () => {
    try {
      const list = await getAccounts();
      setAccounts(list);
      if (list.length > 0 && !selectedAccountId) {
        setSelectedAccountId(list[0].id);
      }
    } catch (err: any) {
      setError(err?.response?.data?.error || '获取账户失败');
    }
  }, [selectedAccountId]);

  // 刷新持仓
  const refreshData = useCallback(async () => {
    if (!selectedAccountId) return;
    setLoading(true);
    try {
      const pos = await getPositions(selectedAccountId);
      setPositions(pos);
      setError('');
    } catch (err: any) {
      setError(err?.response?.data?.error || '获取数据失败');
    } finally {
      setLoading(false);
    }
  }, [selectedAccountId]);

  useEffect(() => { refreshAccounts(); }, [refreshAccounts]);
  useEffect(() => { refreshData(); }, [refreshData]);

  useEffect(() => {
    if (symbol) setOrderSymbol(symbol);
  }, [symbol]);

  const selectedAccount = accounts.find(a => a.id === selectedAccountId);

  const handleCreateAccount = async () => {
    if (!newAccountName.trim()) return;
    try {
      const account = await createAccount({
        name: newAccountName,
        initialBalance: parseFloat(newAccountBalance) || 0,
      });
      setAccounts(prev => [...prev, account]);
      setSelectedAccountId(account.id);
      setShowCreateAccount(false);
      setNewAccountName('');
    } catch (err: any) {
      setError(err?.response?.data?.error || '创建账户失败');
    }
  };

  const handleExecuteTrade = async () => {
    if (!selectedAccountId) { setError('请先选择账户'); return; }
    if (!orderSymbol.trim()) { setError('请输入股票代码'); return; }
    const price = parseFloat(orderPrice);
    const qty = parseFloat(orderQuantity);
    if (!price || !qty || price <= 0 || qty <= 0) { setError('价格和数量必须大于0'); return; }

    try {
      setLoading(true);
      await executeTrade({
        accountId: selectedAccountId,
        symbol: orderSymbol.trim().toUpperCase(),
        direction: orderDirection,
        price,
        quantity: qty,
        fee: parseFloat(orderFee) || 0,
        tax: parseFloat(orderTax) || 0,
        tradeTime: new Date().toISOString(),
      });
      setShowOrderForm(false);
      await refreshData();
      await refreshAccounts(); // 刷新账户余额
    } catch (err: any) {
      setError(err?.response?.data?.error || '交易失败');
    } finally {
      setLoading(false);
    }
  };

  const handleSettlement = async () => {
    if (!selectedAccountId) return;
    try {
      await settlement(selectedAccountId);
      await refreshData();
    } catch (err: any) {
      setError(err?.response?.data?.error || '结算失败');
    }
  };

  const formatMoney = (v: number) => v.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  const formatPct = (v: number) => `${(v * 100).toFixed(2)}%`;

  return (
    <div style={{ width: '100%', height: '100%', background: '#1e222d', display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
      {error && (
        <div style={{ padding: '8px 14px', background: 'rgba(231, 76, 60, 0.15)', color: '#e74c3c', fontSize: 12 }}>
          {error}
        </div>
      )}

      <div style={{ flex: 1, overflowY: 'auto', padding: '12px 14px', display: 'flex', flexDirection: 'column', gap: 14 }}>
        {/* 账户选择 */}
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 8 }}>
            <span style={{ fontSize: 12, color: '#868993', fontWeight: 600 }}>选择账户</span>
            <button
              onClick={async () => {
                if (!selectedAccountId) { setError('请先选择账户'); return; }
                if (!confirm('确定删除该账户？关联的持仓和交易记录将一并删除，此操作不可恢复。')) return;
                try {
                  await deleteAccount(selectedAccountId);
                  setSelectedAccountId(null);
                  await refreshAccounts();
                } catch (err: any) {
                  setError(err?.response?.data?.error || '删除失败');
                }
              }}
              disabled={!selectedAccountId}
              style={{ marginLeft: 'auto', fontSize: 11, padding: '3px 10px', background: '#c0392b', color: '#fff', border: 'none', borderRadius: 4, cursor: selectedAccountId ? 'pointer' : 'not-allowed', opacity: selectedAccountId ? 1 : 0.5 }}
            >删除</button>
            <button
              onClick={() => setShowCreateAccount(true)}
              style={{ fontSize: 11, padding: '3px 10px', background: '#2962FF', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}
            >+ 新建</button>
          </div>
          <select
            value={selectedAccountId ?? ''}
            onChange={(e) => setSelectedAccountId(Number(e.target.value))}
            style={{ width: '100%', padding: '6px 8px', fontSize: 12, background: '#131722', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }}
          >
            {accounts.map(a => (
              <option key={a.id} value={a.id}>{a.name} (可用: {formatMoney(a.availableBalance)})</option>
            ))}
          </select>
        </div>

        {/* 创建账户弹窗 */}
        {showCreateAccount && (
          <div style={{ padding: 12, background: '#131722', borderRadius: 6, border: '1px solid #2a2e39' }}>
            <div style={{ fontSize: 12, color: '#fff', marginBottom: 8 }}>新建账户</div>
            <input value={newAccountName} onChange={e => setNewAccountName(e.target.value)} placeholder="账户名称" style={{ width: '100%', padding: '5px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4, marginBottom: 6 }} />
            <input value={newAccountBalance} onChange={e => setNewAccountBalance(e.target.value)} placeholder="初始资金" type="number" style={{ width: '100%', padding: '5px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4, marginBottom: 8 }} />
            <div style={{ display: 'flex', gap: 8 }}>
              <button onClick={handleCreateAccount} style={{ flex: 1, padding: '5px 0', fontSize: 12, background: '#2962FF', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>确认</button>
              <button onClick={() => setShowCreateAccount(false)} style={{ flex: 1, padding: '5px 0', fontSize: 12, background: '#2a2e39', color: '#d1d4dc', border: 'none', borderRadius: 4, cursor: 'pointer' }}>取消</button>
            </div>
          </div>
        )}

        {/* 账户资金概览 */}
        {selectedAccount && (
          <div style={{ padding: 12, background: '#131722', borderRadius: 6 }}>
            <div style={{ fontSize: 12, color: '#868993', marginBottom: 8 }}>{selectedAccount.name} — 资金概览</div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '6px 12px' }}>
              <div><div style={{ fontSize: 10, color: '#868993' }}>总资产</div><div style={{ fontSize: 13, fontWeight: 600, color: '#d1d4dc' }}>{formatMoney(selectedAccount.totalBalance)}</div></div>
              <div><div style={{ fontSize: 10, color: '#868993' }}>可用余额</div><div style={{ fontSize: 13, fontWeight: 600, color: '#d1d4dc' }}>{formatMoney(selectedAccount.availableBalance)}</div></div>
              <div><div style={{ fontSize: 10, color: '#868993' }}>冻结金额</div><div style={{ fontSize: 13, fontWeight: 600, color: '#d1d4dc' }}>{formatMoney(selectedAccount.frozenBalance)}</div></div>
              <div><div style={{ fontSize: 10, color: '#868993' }}>持仓市值</div><div style={{ fontSize: 13, fontWeight: 600, color: '#d1d4dc' }}>{formatMoney(selectedAccount.marketValue)}</div></div>
              <div style={{ gridColumn: '1 / -1' }}>
                <div style={{ fontSize: 10, color: '#868993' }}>累计盈亏</div>
                <div style={{ fontSize: 13, fontWeight: 600, color: selectedAccount.totalProfitLoss >= 0 ? '#ef5350' : '#26a69a' }}>
                  {selectedAccount.totalProfitLoss >= 0 ? '+' : ''}{formatMoney(selectedAccount.totalProfitLoss)}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* 操作按钮 */}
        <div style={{ display: 'flex', gap: 8 }}>
          <button
            onClick={() => { setOrderDirection('Buy'); setShowOrderForm(true); }}
            disabled={!selectedAccountId || loading}
            style={{ flex: 1, padding: '8px 0', fontSize: 12, fontWeight: 600, background: '#ef5350', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}
          >🔴 买入</button>
          <button
            onClick={() => { setOrderDirection('Sell'); setShowOrderForm(true); }}
            disabled={!selectedAccountId || loading}
            style={{ flex: 1, padding: '8px 0', fontSize: 12, fontWeight: 600, background: '#26a69a', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}
          >🟢 卖出</button>
        </div>

        <button
          onClick={handleSettlement}
          disabled={!selectedAccountId || loading}
          style={{ width: '100%', padding: '6px 0', fontSize: 11, background: '#2a2e39', color: '#868993', border: '1px solid #3a3e49', borderRadius: 4, cursor: 'pointer' }}
        >📋 T+1 结算（释放可用数量）</button>

        {/* 下单弹窗 */}
        {showOrderForm && (
          <div style={{ padding: 12, background: '#131722', borderRadius: 6, border: '1px solid #2a2e39' }}>
            <div style={{ fontSize: 12, color: '#fff', marginBottom: 10, fontWeight: 600 }}>
              {orderDirection === 'Buy' ? '🔴 买入下单' : '🟢 卖出下单'}
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '60px 1fr', gap: '6px 8px', alignItems: 'center', marginBottom: 10 }}>
              <span style={{ fontSize: 11, color: '#868993' }}>股票代码</span>
              <input value={orderSymbol} onChange={e => setOrderSymbol(e.target.value)} style={{ padding: '4px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }} />
              <span style={{ fontSize: 11, color: '#868993' }}>成交价格</span>
              <input value={orderPrice} onChange={e => setOrderPrice(e.target.value)} type="number" step="0.01" style={{ padding: '4px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }} />
              <span style={{ fontSize: 11, color: '#868993' }}>成交数量</span>
              <input value={orderQuantity} onChange={e => setOrderQuantity(e.target.value)} type="number" step="100" style={{ padding: '4px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }} />
              <span style={{ fontSize: 11, color: '#868993' }}>手续费</span>
              <input value={orderFee} onChange={e => setOrderFee(e.target.value)} type="number" step="0.01" style={{ padding: '4px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }} />
              <span style={{ fontSize: 11, color: '#868993' }}>印花税</span>
              <input value={orderTax} onChange={e => setOrderTax(e.target.value)} type="number" step="0.01" style={{ padding: '4px 8px', fontSize: 12, background: '#1e222d', border: '1px solid #2a2e39', color: '#d1d4dc', borderRadius: 4 }} />
            </div>
            <div style={{ fontSize: 11, color: '#868993', marginBottom: 10 }}>
              预计金额: {orderPrice && orderQuantity ? formatMoney(parseFloat(orderPrice) * parseFloat(orderQuantity)) : '0.00'}
            </div>
            <div style={{ display: 'flex', gap: 8 }}>
              <button onClick={handleExecuteTrade} disabled={loading} style={{ flex: 1, padding: '6px 0', fontSize: 12, background: orderDirection === 'Buy' ? '#ef5350' : '#26a69a', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                {loading ? '执行中...' : '确认'}
              </button>
              <button onClick={() => setShowOrderForm(false)} style={{ flex: 1, padding: '6px 0', fontSize: 12, background: '#2a2e39', color: '#d1d4dc', border: 'none', borderRadius: 4, cursor: 'pointer' }}>取消</button>
            </div>
          </div>
        )}

        {/* 持仓列表 */}
        <div>
          <div style={{ fontSize: 12, color: '#868993', fontWeight: 600, marginBottom: 8 }}>📦 当前持仓 ({positions.length})</div>
          {positions.length === 0 ? (
            <div style={{ fontSize: 11, color: '#5a5e69', padding: '8px 0' }}>暂无持仓</div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
              {positions.map(p => (
                <div key={p.id} style={{ padding: 8, background: '#131722', borderRadius: 4, fontSize: 11 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                    <span style={{ color: '#fff', fontWeight: 600 }}>{p.symbol}</span>
                    <span style={{ color: p.profitLoss >= 0 ? '#ef5350' : '#26a69a' }}>
                      {p.profitLoss >= 0 ? '+' : ''}{formatMoney(p.profitLoss)} ({formatPct(p.profitLossRate)})
                    </span>
                  </div>
                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 4, color: '#868993' }}>
                    <span>数量: {p.quantity}</span>
                    <span>可用: {p.availableQuantity}</span>
                    <span>成本: {p.averageCost.toFixed(2)}</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>


      </div>
    </div>
  );
}
