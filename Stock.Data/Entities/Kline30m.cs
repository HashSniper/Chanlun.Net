namespace Stock.Data.Entities;

/// <summary>
/// 30分钟K线数据
/// </summary>
public class Kline30m : KlineBase
{
    public Kline30m()
    {
        Resolution = KlineResolution.Minute30;
    }
}
