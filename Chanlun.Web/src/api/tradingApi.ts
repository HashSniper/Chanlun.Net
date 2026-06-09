import axios from 'axios';
import type { TradingAccount, StockPosition, StockTradeRecord, CreateAccountRequest, ExecuteTradeRequest } from '../types/trading';

const api = axios.create({
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  config.baseURL = localStorage.getItem('chanlun_api_url')?.replace(/\/$/, '') || 'http://localhost:5000';
  return config;
});

// 账户管理
export async function getAccounts(): Promise<TradingAccount[]> {
  const resp = await api.get<TradingAccount[]>('/api/trading/accounts');
  return resp.data;
}

export async function createAccount(req: CreateAccountRequest): Promise<TradingAccount> {
  const resp = await api.post<TradingAccount>('/api/trading/accounts', req);
  return resp.data;
}

export async function deleteAccount(id: number): Promise<{ message: string }> {
  const resp = await api.delete<{ message: string }>(`/api/trading/accounts/${id}`);
  return resp.data;
}

export async function getAccount(id: number): Promise<{ account: TradingAccount; positionCount: number; recordCount: number }> {
  const resp = await api.get(`/api/trading/accounts/${id}`);
  return resp.data;
}

// 持仓
export async function getPositions(accountId: number): Promise<StockPosition[]> {
  const resp = await api.get<StockPosition[]>(`/api/trading/accounts/${accountId}/positions`);
  return resp.data;
}

// 交易记录
export async function getRecords(accountId: number, limit = 100): Promise<StockTradeRecord[]> {
  const resp = await api.get<StockTradeRecord[]>(`/api/trading/accounts/${accountId}/records`, { params: { limit } });
  return resp.data;
}

export async function getAllRecords(limit = 1000): Promise<StockTradeRecord[]> {
  const resp = await api.get<StockTradeRecord[]>('/api/trading/records', { params: { limit } });
  return resp.data;
}

// 执行交易
export async function executeTrade(req: ExecuteTradeRequest): Promise<StockTradeRecord> {
  const resp = await api.post<StockTradeRecord>('/api/trading/execute', req);
  return resp.data;
}

// T+1 结算
export async function settlement(accountId: number): Promise<{ message: string }> {
  const resp = await api.post<{ message: string }>(`/api/trading/settlement/${accountId}`);
  return resp.data;
}
