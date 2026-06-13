using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>BOLL 布林带指标值</summary>
public class BollIndicatorValue
{
    public decimal? Upper { get; set; }
    public decimal? Middle { get; set; }
    public decimal? Lower { get; set; }
}
