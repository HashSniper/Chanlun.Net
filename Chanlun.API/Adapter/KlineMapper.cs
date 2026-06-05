using Chanlun.Lib.KLine;
using Stock.Data.Entities;

namespace Chanlun.API.Adapter;

/// <summary>
/// 将 Chanlun.Lib 的 KLineUnit 映射为 Stock.Data 的 KlineBase 具体实体
/// </summary>
public static class KlineMapper
{
    /// <summary>
    /// 将 KLineUnit 列表映射为对应周期的 KlineBase 实体列表
    /// </summary>
    /// <param name="units">KLineUnit 列表</param>
    /// <param name="symbol">股票代码</param>
    /// <returns>KlineBase 列表（具体类型由时间差自动判断）</returns>
    public static List<KlineBase> Map(List<KLineUnit> units, string symbol)
    {
        if (units == null || units.Count == 0)
            return [];

        var resolution = DetectResolution(units);
        return units.Select(u => MapUnit(u, symbol, resolution)).ToList();
    }

    /// <summary>
    /// 通过相邻 K 线的时间差中位数判断周期级别
    /// </summary>
    private static KlineResolution DetectResolution(List<KLineUnit> units)
    {
        if (units.Count < 2)
            return KlineResolution.Day;

        var diffs = units.Zip(units.Skip(1), (a, b) => b.Time - a.Time)
                         .Select(d => d.TotalMinutes)
                         .ToList();

        // 取中位数，避免休市间隙等异常值干扰
        var medianDiff = diffs.OrderBy(d => d).ElementAt(diffs.Count / 2);

        if (medianDiff < 2)
            return KlineResolution.Minute1;
        if (medianDiff < 10)
            return KlineResolution.Minute5;
        if (medianDiff < 22)
            return KlineResolution.Minute15;
        if (medianDiff < 45)
            return KlineResolution.Minute30;
        if (medianDiff < 90)
            return KlineResolution.Minute60;

        var medianDays = medianDiff / 1440.0;

        if (medianDays < 2)
            return KlineResolution.Day;
        if (medianDays < 10)
            return KlineResolution.Week;

        return KlineResolution.Month;
    }

    /// <summary>
    /// 将单个 KLineUnit 映射为对应周期的 KlineBase 实体
    /// </summary>
    private static KlineBase MapUnit(KLineUnit unit, string symbol, KlineResolution resolution)
    {
        KlineBase result = resolution switch
        {
            KlineResolution.Minute1 => new Kline1m(),
            KlineResolution.Minute5 => new Kline5m(),
            KlineResolution.Minute15 => new Kline15m(),
            KlineResolution.Minute30 => new Kline30m(),
            KlineResolution.Minute60 => new Kline60m(),
            KlineResolution.Day => new Kline1d(),
            KlineResolution.Week => new Kline1w(),
            KlineResolution.Month => new Kline1mo(),
            _ => new Kline1d()
        };

        result.Symbol = symbol;
        result.TradeTime = unit.Time;
        result.Open = unit.Open;
        result.High = unit.High;
        result.Low = unit.Low;
        result.Close = unit.Close;
        result.Volume = unit.Volume;
        result.Amount = unit.Amount;

        return result;
    }
}
