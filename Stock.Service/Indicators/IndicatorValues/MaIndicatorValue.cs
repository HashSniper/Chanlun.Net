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
