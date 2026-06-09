namespace Stock.Data.Entities;

/// <summary>
/// 股票持仓 —— 记录某账户当前持有的某只股票
/// </summary>
public class StockPosition
{
    public long Id { get; set; }

    /// <summary>所属账户ID</summary>
    public long AccountId { get; set; }

    /// <summary>所属账户</summary>
    public TradingAccount Account { get; set; } = null!;

    /// <summary>股票代码，如 SH600000</summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>持仓数量</summary>
    public decimal Quantity { get; set; }

    /// <summary>可用数量（可卖出数量）</summary>
    public decimal AvailableQuantity { get; set; }

    /// <summary>平均成本价</summary>
    public decimal AverageCost { get; set; }

    /// <summary>总成本 = 平均成本 × 数量</summary>
    public decimal TotalCost { get; set; }

    /// <summary>盈亏金额 = 市值 - 总成本</summary>
    public decimal ProfitLoss { get; set; }

    /// <summary>盈亏比例 = 盈亏金额 / 总成本</summary>
    public decimal ProfitLossRate { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
