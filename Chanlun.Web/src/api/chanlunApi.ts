import axios from 'axios';
import type { ChanlunResponse, KlineResolution } from '../types/chanlun';

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


export async function getChanlunKlines(
  symbol: string,
  resolution: KlineResolution,
  from: string,
  to: string
): Promise<ChanlunResponse> {
  const resp = await api.get<ChanlunResponse>('/api/tradingview/chanlunklines', {
    params: { symbol, resolution, from, to },
  });
  return resp.data;
}

export async function getTdxChanlunKlines(): Promise<ChanlunResponse> {
  const resp = await api.get<ChanlunResponse>('/api/tradingview/tdxchanlunklines');
  return resp.data;
}

export async function getUdfConfig() {
  const resp = await api.get('/api/tradingview/config');
  return resp.data;
}
