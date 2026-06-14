using Skender.Stock.Indicators;
using Stock.Data.Entities;
using Stock.Service.Entitys;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// MACD 指标处理器 —— 计算 DIF / DEA / Histogram
/// </summary>
public class MacdIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "MACD";

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        var bars = klines.Select(k => new ServiceQuote(
            k.TradeTime, k.Open, k.High, k.Low, k.Close, k.Volume
        )).ToList();

        var macds = bars.GetMacd().ToList();

        for (int i = 0; i < klines.Count; i++)
        {
            items[i].Macd = macds[i];
        }

        return Task.CompletedTask;
    }
}
