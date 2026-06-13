using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>RSI 相对强弱指标值</summary>
public class RsiIndicatorValue
{
    public decimal? Rsi6 { get; set; }
    public decimal? Rsi12 { get; set; }
    public decimal? Rsi24 { get; set; }
}
