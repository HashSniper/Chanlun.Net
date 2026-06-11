using Skender.Stock.Indicators;
using Stock.Data.Entities;
using Stock.Service.Entitys;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// KDJ 随机指标处理器 —— 计算 K / D / J，以及金叉/死叉 + 背离组合信号
/// </summary>
public class KdjIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "KDJ";

    /// <summary>低位区域阈值（K、D 均低于此值视为低位）</summary>
    private const decimal LowZoneThreshold = 20m;

    /// <summary>高位区域阈值（K、D 均高于此值视为高位）</summary>
    private const decimal HighZoneThreshold = 80m;

    /// <summary>背离检测回溯窗口（根数）</summary>
    private const int DivergenceLookback = 30;

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        if (klines.Count == 0)
            return Task.CompletedTask;

        var bars = klines.Select(k => new ServiceQuote(
            k.TradeTime, k.Open, k.High, k.Low, k.Close, k.Volume
        )).ToList();

        var stochs = bars.GetStoch().ToList();

        // 第一步：填充 K/D/J 基础值
        for (int i = 0; i < klines.Count; i++)
        {
            items[i].Kdj.K = IndicatorUtils.ToDecimal(stochs[i].Oscillator);
            items[i].Kdj.D = IndicatorUtils.ToDecimal(stochs[i].Signal);
            items[i].Kdj.J = IndicatorUtils.CalculateJ(stochs[i].Oscillator, stochs[i].Signal);
        }

        // 第二步：检测金叉/死叉 + 背离，生成信号
        for (int i = 1; i < klines.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var currK = items[i].Kdj.K;
            var currD = items[i].Kdj.D;
            var prevK = items[i - 1].Kdj.K;
            var prevD = items[i - 1].Kdj.D;

            if (currK == null || currD == null || prevK == null || prevD == null)
                continue;

            bool isGoldenCross = prevK < prevD && currK >= currD;
            bool isDeathCross = prevK > prevD && currK <= currD;

            items[i].Kdj.IsGoldenCross = isGoldenCross;
            items[i].Kdj.IsDeathCross = isDeathCross;

            // 检测背离
            if (isGoldenCross)
            {
                items[i].Kdj.IsBottomDivergence = DetectBottomDivergence(klines, items, i);
            }
            else if (isDeathCross)
            {
                items[i].Kdj.IsTopDivergence = DetectTopDivergence(klines, items, i);
            }

            // 生成组合信号（仅在金叉/死叉发生时生成）
            if (isGoldenCross || isDeathCross)
            {
                (items[i].Kdj.Signal, items[i].Kdj.IsBullish) =
                    GenerateSignal(items[i].Kdj, currK.Value, currD.Value, isGoldenCross, isDeathCross);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 检测底背离：当前金叉位置，价格低于前低点，但 KDJ(K) 高于前低点对应值
    /// </summary>
    private static bool DetectBottomDivergence(List<KlineBase> klines, List<KLineIndicatorItem> items, int index)
    {
        int start = Math.Max(0, index - DivergenceLookback);
        if (start >= index) return false;

        // 找前低：回溯窗口内 Close 最低的位置
        int lowIndex = start;
        decimal lowPrice = klines[start].Close;
        for (int j = start + 1; j < index; j++)
        {
            if (klines[j].Close < lowPrice)
            {
                lowPrice = klines[j].Close;
                lowIndex = j;
            }
        }

        var currK = items[index].Kdj.K;
        var prevLowK = items[lowIndex].Kdj.K;

        if (currK == null || prevLowK == null)
            return false;

        // 底背离：价格创新低，KDJ(K) 未创新低
        return klines[index].Close < lowPrice && currK > prevLowK;
    }

    /// <summary>
    /// 检测顶背离：当前死叉位置，价格高于前高点，但 KDJ(K) 低于前高点对应值
    /// </summary>
    private static bool DetectTopDivergence(List<KlineBase> klines, List<KLineIndicatorItem> items, int index)
    {
        int start = Math.Max(0, index - DivergenceLookback);
        if (start >= index) return false;

        // 找前高：回溯窗口内 Close 最高的位置
        int highIndex = start;
        decimal highPrice = klines[start].Close;
        for (int j = start + 1; j < index; j++)
        {
            if (klines[j].Close > highPrice)
            {
                highPrice = klines[j].Close;
                highIndex = j;
            }
        }

        var currK = items[index].Kdj.K;
        var prevHighK = items[highIndex].Kdj.K;

        if (currK == null || prevHighK == null)
            return false;

        // 顶背离：价格创新高，KDJ(K) 未创新高
        return klines[index].Close > highPrice && currK < prevHighK;
    }

    /// <summary>
    /// 生成 KDJ 组合信号
    /// </summary>
    private static (string signal, bool? isBullish) GenerateSignal(
        KdjIndicatorValue kdj, decimal k, decimal d, bool isGoldenCross, bool isDeathCross)
    {
        bool isLowZone = k < LowZoneThreshold && d < LowZoneThreshold;
        bool isHighZone = k > HighZoneThreshold && d > HighZoneThreshold;

        // 低位金叉 + 底背离 → 强烈看涨
        if (isGoldenCross && kdj.IsBottomDivergence)
        {
            return ($"🟢 KDJ低位金叉+底背离：K={k:F2}, D={d:F2}, J={kdj.J:F2}，价格在创新低的同时指标拒绝创新低，强烈看涨信号", true);
        }

        // 高位死叉 + 顶背离 → 强烈看跌
        if (isDeathCross && kdj.IsTopDivergence)
        {
            return ($"🔴 KDJ高位死叉+顶背离：K={k:F2}, D={d:F2}, J={kdj.J:F2}，价格在创新高的同时指标拒绝创新高，强烈看跌信号", false);
        }

        // 低位金叉 → 看涨
        if (isGoldenCross)
        {
            if (isLowZone)
                return ($"🟡 KDJ低位金叉：K={k:F2}, D={d:F2}, J={kdj.J:F2}，处于超卖区，短期反弹概率较大", true);
            return ($"📈 KDJ金叉：K={k:F2}, D={d:F2}, J={kdj.J:F2}，短期趋势转多", true);
        }

        // 高位死叉 → 看跌
        if (isDeathCross)
        {
            if (isHighZone)
                return ($"🟡 KDJ高位死叉：K={k:F2}, D={d:F2}, J={kdj.J:F2}，处于超买区，短期回调概率较大", false);
            return ($"📉 KDJ死叉：K={k:F2}, D={d:F2}, J={kdj.J:F2}，短期趋势转空", false);
        }

        return (string.Empty, null);
    }
}
