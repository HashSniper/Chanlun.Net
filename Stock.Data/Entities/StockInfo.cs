namespace Stock.Data.Entities;

/// <summary>
/// 股票基本信息
/// </summary>
public class StockInfo
{
    public int Id { get; set; }

    /// <summary>
    /// 股票代码，如 SH600000
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// 股票名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 交易所：SH、SZ、BJ
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// 类型：stock、index、etf
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
