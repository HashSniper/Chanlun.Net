using Skender.Stock.Indicators;

namespace Chanlun.Lib.StockIndicators;

public class StockBar : IQuote
{
    public StockBar(DateTime time, decimal open, decimal high, decimal low, decimal close, decimal volume)
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