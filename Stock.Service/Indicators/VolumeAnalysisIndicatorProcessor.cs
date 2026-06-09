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
        // 预计算：量比、成交量变化率、OBV、OBV短期均线
        var volumeRatios = new decimal?[klines.Count];
        var volumeChangePcts = new decimal?[klines.Count];
        var obvs = new decimal?[klines.Count];

        for (int i = 0; i < klines.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

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

        // 计算 OBV 5日均线，用于判断 OBV 趋势方向
        var obvSma5 = new decimal?[klines.Count];
        for (int i = 4; i < klines.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var sum = 0m;
            bool allValid = true;
            for (int j = i - 4; j <= i; j++)
            {
                if (obvs[j] == null) { allValid = false; break; }
                sum += obvs[j]!.Value;
            }
            if (allValid)
                obvSma5[i] = sum / 5m;
        }

        // 填充结果 + 价量配合分析（含 OBV 趋势）
        for (int i = 0; i < klines.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var curr = klines[i];
            KlineBase? prev = i > 0 ? klines[i - 1] : null;

            items[i].Volume.VolumeRatio5 = volumeRatios[i];
            items[i].Volume.VolumeChangePct = volumeChangePcts[i];
            items[i].Volume.Obv = obvs[i];

            // OBV 趋势方向：通过 5日均线斜率判断
            int? obvTrend = null; // 1=向上, -1=向下, 0=走平
            if (i >= 5 && obvSma5[i] != null && obvSma5[i - 1] != null)
            {
                var diff = obvSma5[i]!.Value - obvSma5[i - 1]!.Value;
                if (diff > 0) obvTrend = 1;
                else if (diff < 0) obvTrend = -1;
                else obvTrend = 0;
            }

            (items[i].Volume.VolumeSignal, items[i].Volume.VolumeBullish) =
                AnalyzeVolumePrice(curr, prev, volumeRatios[i], obvTrend);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 分析价量配合关系 + OBV 能量潮与股价的背离/同步，返回信号说明和是否看涨
    ///
    /// 优先级规则：OBV（趋势型领先指标） > 量比（时点型指标）
    /// 当 OBV 与量比结论相反时，以 OBV 为准，量比信息作为辅助参考
    /// </summary>
    /// <param name="curr">当前K线</param>
    /// <param name="prev">前一根K线</param>
    /// <param name="volumeRatio5">5日量比，null表示数据不足</param>
    /// <param name="obvTrend">
    /// OBV 5日均线趋势方向：1=向上，-1=向下，0=走平，null=数据不足
    /// </param>
    private static (string signal, bool? isBullish) AnalyzeVolumePrice(
        KlineBase curr, KlineBase? prev, decimal? volumeRatio5, int? obvTrend)
    {
        if (volumeRatio5 == null)
            return ("数据不足（需至少5根K线），无法判断量能关系", null);

        var ratio = volumeRatio5.Value;
        bool isBullishCandle = curr.Close > curr.Open;
        bool isBearishCandle = curr.Close < curr.Open;
        bool priceUpFromPrev = prev != null && curr.Close > prev.Close;
        bool priceDownFromPrev = prev != null && curr.Close < prev.Close;

        bool priceRising = isBullishCandle || priceUpFromPrev;
        bool priceFalling = isBearishCandle || priceDownFromPrev;

        // ── 第一步：量比分析，得出基础信号 ──
        (string vrSignal, bool? vrBullish) = GetVolumeRatioSignal(ratio, isBullishCandle, isBearishCandle, priceUpFromPrev, priceDownFromPrev);

        // ── 第二步：OBV 趋势分析 ──
        (string? obvLabel, bool? obvBullish, bool isConflict) = GetObvAnalysis(obvTrend, priceRising, priceFalling);

        // ── 第三步：合并信号，OBV 优先 ──
        return MergeSignals(vrSignal, vrBullish, obvLabel, obvBullish, isConflict);
    }

    /// <summary>
    /// 基于 5日量比 生成基础价量信号（仅量比维度，不含 OBV）
    /// </summary>
    private static (string signal, bool? bullish) GetVolumeRatioSignal(
        decimal ratio, bool isBullishCandle, bool isBearishCandle, bool priceUp, bool priceDown)
    {
        // 1. 巨量（异常放大）
        if (ratio >= 2.5m)
        {
            if (isBullishCandle || priceUp)
                return ("巨量拉升：成交量异常放大且价格上涨，注意主力拉高出货或强势突破，需结合位置判断", null);
            if (isBearishCandle || priceDown)
                return ("巨量杀跌：成交量异常放大且价格下跌，恐慌盘涌出或主力砸盘", false);
            return ("巨量成交：成交量异常放大，市场分歧剧烈，需警惕变盘", null);
        }

        // 2. 严重缩量
        if (ratio <= 0.5m)
        {
            if (isBullishCandle || priceUp)
                return ("严重缩量上涨：成交极度萎缩但价格上涨，筹码锁定良好或上涨动力枯竭", null);
            if (isBearishCandle || priceDown)
                return ("严重缩量下跌：成交极度萎缩且价格下跌，抛压枯竭，可能即将企稳反弹", true);
            return ("严重缩量：成交极度低迷，市场观望情绪浓厚，等待方向选择", null);
        }

        // 3. 明显放量 (1.5 ~ 2.5)
        if (ratio >= 1.5m)
        {
            if (isBullishCandle || priceUp)
                return ("量价齐升：价格上涨伴随明显放量，买盘积极", true);
            if (isBearishCandle || priceDown)
                return ("放量下跌：价格下跌伴随明显放量，卖盘涌出", false);
            return ("明显放量：成交量显著放大，多空博弈激烈", null);
        }

        // 4. 明显缩量 (0.5 ~ 0.8)
        if (ratio <= 0.8m)
        {
            if (isBullishCandle || priceUp)
                return ("缩量上涨：价格上涨但量能不足，上涨动力减弱，量价背离", false);
            if (isBearishCandle || priceDown)
                return ("缩量下跌：价格下跌但量能萎缩，抛压减轻，可能是洗盘或企稳信号", true);
            return ("明显缩量：成交量萎缩，市场参与度降低", null);
        }

        // 5. 正常量能 (0.8 ~ 1.5)
        if (isBullishCandle || priceUp)
            return ("量平价涨：价格温和上涨，量能正常", true);
        if (isBearishCandle || priceDown)
            return ("量平价跌：价格温和下跌，量能正常", false);

        return ("量平价平：价格波动不大，量能正常", null);
    }

    /// <summary>
    /// 分析 OBV 趋势与股价关系，返回 OBV 标签、多空方向、是否与量比信号冲突
    /// </summary>
    private static (string? label, bool? bullish, bool isConflict) GetObvAnalysis(
        int? obvTrend, bool priceRising, bool priceFalling)
    {
        if (obvTrend == null)
            return (null, null, false);

        // 同步：OBV 与股价方向一致 → 确认信号，不冲突
        if (priceRising && obvTrend == 1)
            return ("OBV同步上行，量在价先，确认看涨", true, false);
        if (priceFalling && obvTrend == -1)
            return ("OBV同步下行，资金持续流出，确认看跌", false, false);

        // 背离：OBV 与股价方向相反 → 冲突！OBV 优先
        if (priceRising && obvTrend == -1)
            return ("OBV顶背离：价格上行但能量潮回落，上涨缺乏资金支持", false, true);
        if (priceFalling && obvTrend == 1)
            return ("OBV底背离：价格下行但能量潮抬升，有资金暗中吸筹", true, true);

        // 无明确价格方向时的 OBV 趋势
        if (obvTrend == 1)
            return ("OBV能量潮上行，资金流入迹象", true, false);
        if (obvTrend == -1)
            return ("OBV能量潮下行，资金流出迹象", false, false);

        return ("OBV能量潮走平，资金观望", null, false);
    }

    /// <summary>
    /// 合并量比信号和 OBV 信号，当冲突时以 OBV 为准
    /// </summary>
    private static (string signal, bool? bullish) MergeSignals(
        string vrSignal, bool? vrBullish,
        string? obvLabel, bool? obvBullish, bool isConflict)
    {
        // 无 OBV 数据 → 仅输出量比信号
        if (obvLabel == null)
            return ($"{vrSignal}，观望", vrBullish);

        if (isConflict)
        {
            // OBV 与量比冲突 → OBV 为主结论，量比为辅助说明
            bool? finalBullish = obvBullish;
            string conclusion = finalBullish == true ? "看涨" : finalBullish == false ? "看跌" : "观望";
            return ($"⚠ {obvLabel}，以能量潮为准【{conclusion}】；当日{vrSignal}为短期现象", finalBullish);
        }

        // OBV 与量比一致 → 量比为主，OBV 为辅
        string vrConclusion = vrBullish == true ? "看涨" : vrBullish == false ? "看跌" : "观望";
        return ($"{vrSignal}，{vrConclusion}；{obvLabel}", vrBullish);
    }
}
