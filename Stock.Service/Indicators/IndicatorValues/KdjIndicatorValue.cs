using Stock.Service.Interface;

namespace Stock.Service.Indicators;

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
