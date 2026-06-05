namespace Stock.Data.Entities;

/// <summary>
/// 1分钟K线数据
/// </summary>
public class Kline1m : KlineBase
{
    public Kline1m()
    {
        Resolution = KlineResolution.Minute1;
    }
}
