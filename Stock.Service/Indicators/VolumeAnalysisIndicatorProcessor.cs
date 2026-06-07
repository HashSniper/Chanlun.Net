using Stock.Data.Entities;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// 量能分析指标处理器 —— 计算量比、OBV、价量配合信号
/// </summary>
public class VolumeAnalysisIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "VolumeAnalysis";

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        // 预计算：量比、成交量变化率、OBV
        var volumeRatios = new decimal?[klines.Count];
        var volumeChangePcts = new decimal?[klines.Count];
        var obvs = new decimal?[klines.Count];

        for (int i = 0; i < klines.Count; i++)
        {
            // 5日量比
            if (i >= 5)
            {
                var avgVolume5 = klines.Skip(i - 5).Take(5).Average(k => k.Volume);
                if (avgVolume5 > 0)
                    volumeRatios[i] = klines[i].Volume / avgVolume5;
            }

            // 相对前一日成交量变化率
            if (i > 0 && klines[i - 1].Volume > 0)
            {
                volumeChangePcts[i] = (klines[i].Volume - klines[i - 1].Volume) / klines[i - 1].Volume * 100;
            }

            // OBV 能量潮
            if (i == 0)
            {
                obvs[i] = klines[i].Volume;
            }
            else
            {
                var prevObv = obvs[i - 1] ?? 0;
                if (klines[i].Close > klines[i - 1].Close)
                    obvs[i] = prevObv + klines[i].Volume;
                else if (klines[i].Close < klines[i - 1].Close)
                    obvs[i] = prevObv - klines[i].Volume;
                else
                    obvs[i] = prevObv;
            }
        }

        // 填充结果 + 价量配合分析
        for (int i = 0; i < klines.Count; i++)
        {
            var curr = klines[i];
            KlineBase? prev = i > 0 ? klines[i - 1] : null;

            items[i].Volume.VolumeRatio5 = volumeRatios[i];
            items[i].Volume.VolumeChangePct = volumeChangePcts[i];
            items[i].Volume.Obv = obvs[i];

            (items[i].Volume.VolumeSignal, items[i].Volume.VolumeBullish) = AnalyzeVolumePrice(curr, prev, volumeRatios[i]);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 分析价量配合关系，返回信号说明和是否看涨
    /// </summary>
    private static (string signal, bool? isBullish) AnalyzeVolumePrice(KlineBase curr, KlineBase? prev, decimal? volumeRatio5)
    {
        if (volumeRatio5 == null)
            return ("数据不足（需至少5根K线），无法判断量能关系", null);

        var ratio = volumeRatio5.Value;
        bool isBullishCandle = curr.Close > curr.Open;
        bool isBearishCandle = curr.Close < curr.Open;
        bool priceUpFromPrev = prev != null && curr.Close > prev.Close;
        bool priceDownFromPrev = prev != null && curr.Close < prev.Close;

        // 1. 巨量（异常放大）
        if (ratio >= 2.5m)
        {
            if (isBullishCandle || priceUpFromPrev)
                return ("巨量拉升：成交量异常放大且价格上涨，注意主力拉高出货或强势突破，需结合位置判断", null);
            if (isBearishCandle || priceDownFromPrev)
                return ("巨量杀跌：成交量异常放大且价格下跌，恐慌盘涌出或主力砸盘，看跌", false);
            return ("巨量成交：成交量异常放大，市场分歧剧烈，需警惕变盘", null);
        }

        // 2. 严重缩量
        if (ratio <= 0.5m)
        {
            if (isBullishCandle || priceUpFromPrev)
                return ("缩量上涨：成交极度萎缩但价格上涨，筹码锁定良好或上涨动力枯竭，观望", null);
            if (isBearishCandle || priceDownFromPrev)
                return ("缩量下跌：成交极度萎缩且价格下跌，抛压枯竭，可能即将企稳反弹", true);
            return ("严重缩量：成交极度低迷，市场观望情绪浓厚，等待方向选择", null);
        }

        // 3. 明显放量 (1.5 ~ 2.5)
        if (ratio >= 1.5m)
        {
            if (isBullishCandle || priceUpFromPrev)
                return ("量价齐升：价格上涨伴随明显放量，买盘积极，资金流入，看涨", true);
            if (isBearishCandle || priceDownFromPrev)
                return ("放量下跌：价格下跌伴随明显放量，卖盘涌出，资金出逃，看跌", false);
            return ("明显放量：成交量显著放大，多空博弈激烈", null);
        }

        // 4. 明显缩量 (0.5 ~ 0.8)
        if (ratio <= 0.8m)
        {
            if (isBullishCandle || priceUpFromPrev)
                return ("缩量上涨：价格上涨但量能不足，上涨动力减弱，量价背离，谨慎", false);
            if (isBearishCandle || priceDownFromPrev)
                return ("缩量下跌：价格下跌但量能萎缩，抛压减轻，可能是洗盘或企稳信号", true);
            return ("明显缩量：成交量萎缩，市场参与度降低", null);
        }

        // 5. 正常量能 (0.8 ~ 1.5)
        if (isBullishCandle || priceUpFromPrev)
            return ("量平价涨：价格温和上涨，量能正常，趋势健康，偏多", true);
        if (isBearishCandle || priceDownFromPrev)
            return ("量平价跌：价格温和下跌，量能正常，趋势延续，偏空", false);

        return ("量平价平：价格波动不大，量能正常，市场观望", null);
    }
}
