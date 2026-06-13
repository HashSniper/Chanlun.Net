using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>MACD 指标值</summary>
public class MacdIndicatorValue
{
    public decimal? Dif { get; set; }
    public decimal? Dea { get; set; }
    public decimal? Histogram { get; set; }
}
