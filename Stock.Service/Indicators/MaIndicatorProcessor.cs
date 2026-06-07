using Skender.Stock.Indicators;
using Stock.Data.Entities;
using Stock.Service.Entitys;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// MA 移动平均线指标处理器 —— 计算 SMA 5/10/20/60
/// </summary>
public class MaIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "MA";

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        var bars = klines.Select(k => new ServiceQuote(
            k.TradeTime, k.Open, k.High, k.Low, k.Close, k.Volume
        )).ToList();

        var sma5 = bars.GetSma(5).ToList();
        var sma10 = bars.GetSma(10).ToList();
        var sma20 = bars.GetSma(20).ToList();
        var sma60 = bars.GetSma(60).ToList();

        for (int i = 0; i < klines.Count; i++)
        {
            items[i].Ma.MA5 = IndicatorUtils.ToDecimal(sma5[i].Sma);
            items[i].Ma.MA10 = IndicatorUtils.ToDecimal(sma10[i].Sma);
            items[i].Ma.MA20 = IndicatorUtils.ToDecimal(sma20[i].Sma);
            items[i].Ma.MA60 = IndicatorUtils.ToDecimal(sma60[i].Sma);
        }

        return Task.CompletedTask;
    }
}
