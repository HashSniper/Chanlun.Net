namespace Stock.Data.Entities;

/// <summary>
/// 60分钟K线数据
/// </summary>
public class Kline60m : KlineBase
{
    public Kline60m()
    {
        Resolution = KlineResolution.Minute60;
    }
}
