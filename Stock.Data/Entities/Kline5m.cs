namespace Stock.Data.Entities;

/// <summary>
/// 5分钟K线数据
/// </summary>
public class Kline5m : KlineBase
{
    public Kline5m()
    {
        Resolution = KlineResolution.Minute5;
    }
}
