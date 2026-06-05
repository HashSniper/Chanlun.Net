namespace Stock.Data.Entities;

/// <summary>
/// 15分钟K线数据
/// </summary>
public class Kline15m : KlineBase
{
    public Kline15m()
    {
        Resolution = KlineResolution.Minute15;
    }
}
