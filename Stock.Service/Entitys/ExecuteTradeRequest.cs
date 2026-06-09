using Stock.Data.Entities;

namespace Stock.Service.Entitys;

/// <summary>
/// 执行交易请求
/// </summary>
public class ExecuteTradeRequest
{
    /// <summary>所属账户ID</summary>
    public long AccountId { get; set; }

    /// <summary>股票代码，如 SH600000</summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>交易方向：买入 / 卖出</summary>
    public TradeDirection Direction { get; set; }

    /// <summary>成交价格</summary>
    public decimal Price { get; set; }

    /// <summary>成交数量</summary>
    public decimal Quantity { get; set; }

    /// <summary>交易佣金/手续费</summary>
    public decimal Fee { get; set; }

    /// <summary>印花税（卖出时通常有）</summary>
    public decimal Tax { get; set; }

    /// <summary>成交时间</summary>
    public DateTime TradeTime { get; set; } = DateTime.Now;

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
