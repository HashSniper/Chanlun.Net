using Skender.Stock.Indicators;
using Stock.Data.Entities;
using Stock.Service.Entitys;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// BOLL 布林带指标处理器 —— 计算 Upper / Middle / Lower
/// </summary>
public class BollIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "BOLL";

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        var bars = klines.Select(k => new ServiceQuote(
            k.TradeTime, k.Open, k.High, k.Low, k.Close, k.Volume
        )).ToList();

        var bolls = bars.GetBollingerBands().ToList();

        for (int i = 0; i < klines.Count; i++)
        {
            items[i].Boll.Upper = IndicatorUtils.ToDecimal(bolls[i].UpperBand);
            items[i].Boll.Middle = IndicatorUtils.ToDecimal(bolls[i].Sma);
            items[i].Boll.Lower = IndicatorUtils.ToDecimal(bolls[i].LowerBand);
        }

        return Task.CompletedTask;
    }
}
