export interface KlineBar {
  time: string;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
  calIndicator?: number;
  // 日本蜡烛图形态
  patterns?: string[];
  patternDirection?: string;
  patternSignal?: string;

  // 量能关系指标
  volumeRatio5?: number;
  volumeChangePct?: number;
  obv?: number;
  volumeSignal?: string;
  volumeBullish?: boolean | null;

  // 海龟交易法则指标
  turtleHigh20?: number;
  turtleHigh50?: number;
  turtleLow20?: number;
  turtleLow50?: number;
  turtleBreakoutHigh20?: boolean;
  turtleBreakoutHigh50?: boolean;
  turtleBreakdownLow20?: boolean;
  turtleBreakdownLow50?: boolean;
  turtleSignal?: string;
  turtleBullish?: boolean | null;
}

export interface BiItem {
  startTime: number;
  startPrice: number;
  endTime: number;
  endPrice: number;
  direction: 'up' | 'down';
}

export interface SegItem {
  startTime: number;
  startPrice: number;
  endTime: number;
  endPrice: number;
  direction: 'up' | 'down';
}

export interface PivotItem {
  startTime: number;
  endTime: number;
  zg: number;
  zd: number;
  gg: number;
  dd: number;
  type: 'bi' | 'seg';
  level: number;
}

export interface MergedKLine {
  startTime: number;
  endTime: number;
  high: number;
  low: number;
  direction: 'up' | 'down' | 'combine';
}

export interface ChanlunResponse {
  symbol: string;
  barCount: number;
  resolution: string;
  fromTime: string;
  toTime: string;
  bars: KlineBar[];
  biList: BiItem[];
  segList: SegItem[];
  biPivotList: PivotItem[];
  segPivotList: PivotItem[];
  mergedKLines: MergedKLine[];
}

export type KlineResolution = 'Minute1' | 'Minute5' | 'Minute15' | 'Minute30' | 'Minute60' | 'Day' | 'Week' | 'Month';

export interface UdfHistory {
  s: 'ok' | 'error' | 'no_data';
  errmsg?: string;
  t?: number[];
  o?: number[];
  h?: number[];
  l?: number[];
  c?: number[];
  v?: number[];
}
