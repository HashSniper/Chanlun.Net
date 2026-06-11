using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>MA 移动平均线指标值</summary>
public class MaIndicatorValue
{
    public decimal? MA5 { get; set; }
    public decimal? MA10 { get; set; }
    public decimal? MA20 { get; set; }
    public decimal? MA60 { get; set; }
}

/// <summary>MACD 指标值</summary>
public class MacdIndicatorValue
{
    public decimal? Dif { get; set; }
    public decimal? Dea { get; set; }
    public decimal? Histogram { get; set; }
}

/// <summary>KDJ 随机指标值</summary>
public class KdjIndicatorValue
{
    public decimal? K { get; set; }
    public decimal? D { get; set; }
    public decimal? J { get; set; }

    /// <summary>KDJ 信号描述</summary>
    public string Signal { get; set; } = string.Empty;

    /// <summary>是否看涨（null=观望）</summary>
    public bool? IsBullish { get; set; }

    /// <summary>是否金叉</summary>
    public bool IsGoldenCross { get; set; }

    /// <summary>是否死叉</summary>
    public bool IsDeathCross { get; set; }

    /// <summary>是否底背离</summary>
    public bool IsBottomDivergence { get; set; }

    /// <summary>是否顶背离</summary>
    public bool IsTopDivergence { get; set; }
}

/// <summary>RSI 相对强弱指标值</summary>
public class RsiIndicatorValue
{
    public decimal? Rsi6 { get; set; }
    public decimal? Rsi12 { get; set; }
    public decimal? Rsi24 { get; set; }
}

/// <summary>BOLL 布林带指标值</summary>
public class BollIndicatorValue
{
    public decimal? Upper { get; set; }
    public decimal? Middle { get; set; }
    public decimal? Lower { get; set; }
}

/// <summary>蜡烛图形态指标值</summary>
public class CandlestickPatternValue
{
    public CandlestickPattern Patterns { get; set; }
    public List<CandlestickPatternInfo> PatternDetails { get; set; } = [];
    public PatternDirection PatternDirection { get; set; }
    public string PatternSignal { get; set; } = string.Empty;
}

/// <summary>量能分析指标值</summary>
public class VolumeAnalysisValue
{
    public decimal? VolumeRatio5 { get; set; }
    public decimal? VolumeChangePct { get; set; }
    public decimal? Obv { get; set; }
    public string VolumeSignal { get; set; } = string.Empty;
    public bool? VolumeBullish { get; set; }
}

/// <summary>海龟交易法则指标值（唐奇安通道突破）</summary>
public class TurtleTradingValue
{
    /// <summary>20日最高价</summary>
    public decimal? High20 { get; set; }

    /// <summary>50日最高价</summary>
    public decimal? High50 { get; set; }

    /// <summary>20日最低价</summary>
    public decimal? Low20 { get; set; }

    /// <summary>50日最低价</summary>
    public decimal? Low50 { get; set; }

    /// <summary>是否突破20日最高点</summary>
    public bool BreakoutHigh20 { get; set; }

    /// <summary>是否突破50日最高点</summary>
    public bool BreakoutHigh50 { get; set; }

    /// <summary>是否跌破20日最低点</summary>
    public bool BreakdownLow20 { get; set; }

    /// <summary>是否跌破50日最低点</summary>
    public bool BreakdownLow50 { get; set; }

    /// <summary>信号描述</summary>
    public string Signal { get; set; } = string.Empty;

    /// <summary>是否看涨（null=观望）</summary>
    public bool? IsBullish { get; set; }
}
