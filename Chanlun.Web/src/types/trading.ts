export interface TradingAccount {
  id: number;
  name: string;
  totalBalance: number;
  availableBalance: number;
  frozenBalance: number;
  marketValue: number;
  totalProfitLoss: number;
  updatedAt: string;
  createdAt: string;
}

export interface StockPosition {
  id: number;
  accountId: number;
  symbol: string;
  quantity: number;
  availableQuantity: number;
  averageCost: number;
  totalCost: number;
  profitLoss: number;
  profitLossRate: number;
  updatedAt: string;
  createdAt: string;
}

export interface StockTradeRecord {
  id: number;
  accountId: number;
  symbol: string;
  direction: 'Buy' | 'Sell';
  price: number;
  quantity: number;
  amount: number;
  fee: number;
  tax: number;
  totalAmount: number;
  tradeTime: string;
  remark: string | null;
  createdAt: string;
}

export interface CreateAccountRequest {
  name: string;
  initialBalance: number;
}

export interface ExecuteTradeRequest {
  accountId: number;
  symbol: string;
  direction: 'Buy' | 'Sell';
  price: number;
  quantity: number;
  fee: number;
  tax: number;
  tradeTime: string;
  remark?: string;
}
