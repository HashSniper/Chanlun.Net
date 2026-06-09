namespace Stock.Data.Entities;

/// <summary>
/// 股票交易记录 —— 记录某账户的每笔买入/卖出成交
/// </summary>
public class StockTradeRecord
{
    public long Id { get; set; }

    /// <summary>所属账户ID</summary>
    public long AccountId { get; set; }

    /// <summary>所属账户</summary>
    public TradingAccount Account { get; set; } = null!;

    /// <summary>股票代码，如 SH600000</summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>交易方向：买入 / 卖出</summary>
    public TradeDirection Direction { get; set; }

    /// <summary>成交价格</summary>
    public decimal Price { get; set; }

    /// <summary>成交数量</summary>
    public decimal Quantity { get; set; }

    /// <summary>成交金额 = 成交价 × 数量</summary>
    public decimal Amount { get; set; }

    /// <summary>交易佣金/手续费</summary>
    public decimal Fee { get; set; }

    /// <summary>印花税（卖出时通常有）</summary>
    public decimal Tax { get; set; }

    /// <summary>总发生金额（买入为负，卖出为正，已含税费）</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>成交时间</summary>
    public DateTime TradeTime { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    /// <summary>记录创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
