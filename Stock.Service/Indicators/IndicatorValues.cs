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
