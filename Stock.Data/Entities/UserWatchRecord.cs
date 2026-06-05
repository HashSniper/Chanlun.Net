namespace Stock.Data.Entities;

/// <summary>
/// 用户当前正在查看的股票记录
/// </summary>
public class UserWatchRecord
{
    public long Id { get; set; }

    /// <summary>股票代码，如 SH600000</summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>K线类型，如 1、5、15、30、60、D、W、M</summary>
    public string Resolution { get; set; } = string.Empty;

    /// <summary>K线开始时间</summary>
    public DateTime StartTime { get; set; }

    /// <summary>K线结束时间</summary>
    public DateTime EndTime { get; set; }

    /// <summary>记录创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
