using Skender.Stock.Indicators;
using Stock.Data.Entities;
using Stock.Service.Entitys;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// KDJ 随机指标处理器 —— 计算 K / D / J
/// </summary>
public class KdjIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "KDJ";

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        var bars = klines.Select(k => new ServiceQuote(
            k.TradeTime, k.Open, k.High, k.Low, k.Close, k.Volume
        )).ToList();

        var stochs = bars.GetStoch().ToList();

        for (int i = 0; i < klines.Count; i++)
        {
            items[i].Kdj.K = IndicatorUtils.ToDecimal(stochs[i].Oscillator);
            items[i].Kdj.D = IndicatorUtils.ToDecimal(stochs[i].Signal);
            items[i].Kdj.J = IndicatorUtils.CalculateJ(stochs[i].Oscillator, stochs[i].Signal);
        }

        return Task.CompletedTask;
    }
}
