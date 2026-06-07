using Skender.Stock.Indicators;

namespace Stock.Service.Entitys;

/// <summary>
/// 内部使用的 Quote 实现，用于 Skender.Stock.Indicators 指标计算
/// </summary>
internal class ServiceQuote : IQuote
{
    public ServiceQuote(DateTime time, decimal open, decimal high, decimal low, decimal close, decimal volume)
    {
        Date = time;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }

    public DateTime Date { get; }
    public decimal Open { get; }
    public decimal High { get; }
    public decimal Low { get; }
    public decimal Close { get; }
    public decimal Volume { get; }
}
