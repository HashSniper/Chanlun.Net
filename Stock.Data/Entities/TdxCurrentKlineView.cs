namespace Stock.Data.Entities;

/// <summary>
/// 通达信用户当前正在查看的K线视图信息
/// </summary>
public class TdxCurrentKlineView
{
    public long Id { get; set; }

    /// <summary>股票代码，如 SH600000</summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>K线类型</summary>
    public KlineResolution Resolution { get; set; }

    /// <summary>K线开始时间</summary>
    public DateTime StartTime { get; set; }

    /// <summary>K线结束时间</summary>
    public DateTime EndTime { get; set; }

    /// <summary>记录创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
