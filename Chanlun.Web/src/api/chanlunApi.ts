import axios from 'axios';
import type { ChanlunResponse, KlineBar, UdfHistory } from '../types/chanlun';

const DEFAULT_BASE_URL = 'http://localhost:5000';

export function getBaseUrl(): string {
  return localStorage.getItem('chanlun_api_url') || DEFAULT_BASE_URL;
}

export function setBaseUrl(url: string) {
  localStorage.setItem('chanlun_api_url', url.replace(/\/$/, ''));
}

const api = axios.create({
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  config.baseURL = getBaseUrl();
  return config;
});

export async function calculateChanlun(symbol: string, bars: KlineBar[]): Promise<ChanlunResponse> {
  const resp = await api.post<ChanlunResponse>('/api/calculation/tvchanlun', {
    symbol,
    bars,
  });
  return resp.data;
}

export async function pushKlines(symbol: string, bars: KlineBar[]): Promise<{ symbol: string; count: number; message: string }> {
  const resp = await api.post('/api/tradingview/push', { symbol, bars });
  return resp.data;
}

export async function getUdfHistory(
  symbol: string,
  resolution: string = 'D',
  from: number,
  to: number
): Promise<UdfHistory> {
  const resp = await api.get<UdfHistory>('/api/tradingview/history', {
    params: { symbol, resolution, from, to },
  });
  return resp.data;
}

export async function getUdfConfig() {
  const resp = await api.get('/api/tradingview/config');
  return resp.data;
}
