namespace Stock.Data.Entities;

/// <summary>
/// K线数据抽象基类
/// </summary>
public abstract class KlineBase
{
    public long Id { get; set; }

    /// <summary>股票代码，如 SH600000</summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>交易时间</summary>
    public DateTime TradeTime { get; set; }

    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }

    /// <summary>成交额</summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// K线周期类型，不存入数据库
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public KlineResolution Resolution { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
