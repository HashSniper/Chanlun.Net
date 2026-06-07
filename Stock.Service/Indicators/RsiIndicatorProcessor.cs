using Skender.Stock.Indicators;
using Stock.Data.Entities;
using Stock.Service.Entitys;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// RSI 相对强弱指标处理器 —— 计算 RSI 6/12/24
/// </summary>
public class RsiIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "RSI";

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        var bars = klines.Select(k => new ServiceQuote(
            k.TradeTime, k.Open, k.High, k.Low, k.Close, k.Volume
        )).ToList();

        var rsi6 = bars.GetRsi(6).ToList();
        var rsi12 = bars.GetRsi(12).ToList();
        var rsi24 = bars.GetRsi(24).ToList();

        for (int i = 0; i < klines.Count; i++)
        {
            items[i].Rsi.Rsi6 = IndicatorUtils.ToDecimal(rsi6[i].Rsi);
            items[i].Rsi.Rsi12 = IndicatorUtils.ToDecimal(rsi12[i].Rsi);
            items[i].Rsi.Rsi24 = IndicatorUtils.ToDecimal(rsi24[i].Rsi);
        }

        return Task.CompletedTask;
    }
}
