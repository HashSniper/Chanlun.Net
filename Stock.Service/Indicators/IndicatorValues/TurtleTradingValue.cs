using Stock.Service.Interface;

namespace Stock.Service.Indicators;

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
