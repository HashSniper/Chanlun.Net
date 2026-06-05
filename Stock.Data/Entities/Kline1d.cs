namespace Stock.Data.Entities;

/// <summary>
/// 日线K线数据
/// </summary>
public class Kline1d : KlineBase
{
    public Kline1d()
    {
        Resolution = KlineResolution.Day;
    }
}
