namespace Stock.Data.Entities;

/// <summary>
/// 交易账户 —— 记录账户总资产、可用资金、持仓市值及总盈亏
/// </summary>
public class TradingAccount
{
    public long Id { get; set; }

    /// <summary>账户名称，如"主账户"、"融资融券"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>账户总资金（可用 + 冻结 + 持仓市值）</summary>
    public decimal TotalBalance { get; set; }

    /// <summary>可用余额，可用于买入</summary>
    public decimal AvailableBalance { get; set; }

    /// <summary>冻结金额，如挂单未成交占用的资金</summary>
    public decimal FrozenBalance { get; set; }

    /// <summary>当前持仓总市值</summary>
    public decimal MarketValue { get; set; }

    /// <summary>累计总盈亏</summary>
    public decimal TotalProfitLoss { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>当前持仓列表</summary>
    public List<StockPosition> Positions { get; set; } = [];

    /// <summary>交易记录列表</summary>
    public List<StockTradeRecord> TradeRecords { get; set; } = [];
}
