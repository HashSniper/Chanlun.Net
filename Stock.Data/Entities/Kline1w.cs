namespace Stock.Data.Entities;

/// <summary>
/// 周线K线数据
/// </summary>
public class Kline1w : KlineBase
{
    public Kline1w()
    {
        Resolution = KlineResolution.Week;
    }
}
