using Stock.Data.Entities;
using Stock.Service.Indicators;
using Stock.Service.Interface;

namespace Stock.Service.Interface;

/// <summary>
/// 单根K线及其对应的技术指标（聚合对象）
/// </summary>
public class KLineIndicatorItem
{
    /// <summary>K线基础数据</summary>
    public KlineBase Kline { get; set; } = null!;

    /// <summary>MA 移动平均线指标值</summary>
    public MaIndicatorValue Ma { get; set; } = new();

    /// <summary>MACD 指标值</summary>
    public MacdIndicatorValue Macd { get; set; } = new();

    /// <summary>KDJ 随机指标值</summary>
    public KdjIndicatorValue Kdj { get; set; } = new();

    /// <summary>RSI 相对强弱指标值</summary>
    public RsiIndicatorValue Rsi { get; set; } = new();

    /// <summary>BOLL 布林带指标值</summary>
    public BollIndicatorValue Boll { get; set; } = new();

    /// <summary>蜡烛图形态指标值</summary>
    public CandlestickPatternValue Candlestick { get; set; } = new();

    /// <summary>量能分析指标值</summary>
    public VolumeAnalysisValue Volume { get; set; } = new();

    /// <summary>海龟交易法则指标值</summary>
    public TurtleTradingValue TurtleTrading { get; set; } = new();
}

/// <summary>
/// K线指标计算结果
/// </summary>
public class KLineIndicatorResult
{
    public string Symbol { get; set; } = string.Empty;
    public KlineResolution Resolution { get; set; }
    public DateTime FromTime { get; set; }
    public DateTime ToTime { get; set; }
    public int Count { get; set; }
    public List<KLineIndicatorItem> Items { get; set; } = [];
}
