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

    /// <summary>
    /// 交易结算类型，默认 T+1
    /// </summary>
    public TradeSettlementType SettlementType { get; set; } = TradeSettlementType.T1;

    /// <summary>
    /// 数据同步状态：0=未同步，1=同步中，2=已同步
    /// </summary>
    public SyncStatus SyncStatus { get; set; } = SyncStatus.NotSynced;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
