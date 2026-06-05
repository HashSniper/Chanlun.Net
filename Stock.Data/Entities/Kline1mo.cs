namespace Stock.Data.Entities;

/// <summary>
/// 月线K线数据
/// </summary>
public class Kline1mo : KlineBase
{
    public Kline1mo()
    {
        Resolution = KlineResolution.Month;
    }
}
