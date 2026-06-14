using BiType = Chanlun.Lib.Bi.Bi;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Zs;

namespace Chanlun.Lib.TradingPoint;

/// <summary>
/// 缠论买卖点计算器（基于笔中枢）
/// </summary>
public static class ChanTradingPointCalculator
{
    public static void Calculate(ref ChanCalculateResult result)
    {
        if (result?.BiList == null)
            return;

        // 清空已有标记
        foreach (var unit in result.UnitList)
        {
            unit.UnitIndicator.IsBuy1 = null;
            unit.UnitIndicator.IsBuy2 = null;
            unit.UnitIndicator.IsBuy3 = null;
            unit.UnitIndicator.IsSell1 = null;
            unit.UnitIndicator.IsSell2 = null;
            unit.UnitIndicator.IsSell3 = null;
        }

        var pivots = result.BiPivotList.Where(p => p.IsClosed).ToList();
        if (pivots.Count == 0)
            return;

        var firstBuySells = new List<(BiType bi, bool isBuy)>();

        // 一买/一卖：中枢背驰点
        foreach (var pivot in pivots)
        {
            var outSegment = pivot.OutSegment;
            if (outSegment == null)
                continue;

            if (pivot.DIR.IsDown() && outSegment.DIR.IsDown())
            {
                // 下跌中枢，退出段创新低且背驰 -> 一买
                if (outSegment.Low < pivot.ZD && IsDivergence(pivot, outSegment))
                {
                    MarkPoint(outSegment.EndChanKLine.PeakUnit, isBuy: true, level: 1);
                    firstBuySells.Add((outSegment, true));
                }
            }
            else if (pivot.DIR.IsUp() && outSegment.DIR.IsUp())
            {
                // 上涨中枢，退出段创新高且背驰 -> 一卖
                if (outSegment.High > pivot.ZG && IsDivergence(pivot, outSegment))
                {
                    MarkPoint(outSegment.EndChanKLine.PeakUnit, isBuy: false, level: 1);
                    firstBuySells.Add((outSegment, false));
                }
            }
        }

        // 二买/二卖：一买/一卖后，先有一个反向笔（反弹/回抽），再有一个回拉笔不创新低/高
        foreach (var (firstBi, isBuy) in firstBuySells)
        {
            // 一买后：向下笔(一买) → 向上反弹笔 → 向下回抽笔(二买)
            // 一卖后：向上笔(一卖) → 向下回抽笔 → 向上反弹笔(二卖)
            var pullback = firstBi.Next?.Next;
            if (pullback == null)
                continue;

            if (isBuy && pullback.DIR.IsDown() && pullback.Low >= firstBi.Low)
            {
                MarkPoint(pullback.EndChanKLine.PeakUnit, isBuy: true, level: 2);
            }
            else if (!isBuy && pullback.DIR.IsUp() && pullback.High <= firstBi.High)
            {
                MarkPoint(pullback.EndChanKLine.PeakUnit, isBuy: false, level: 2);
            }
        }

        // 三买/三卖：离开中枢后回拉不进中枢区间
        foreach (var pivot in pivots)
        {
            var outSegment = pivot.OutSegment;
            if (outSegment == null)
                continue;

            var pullback = outSegment.Next;
            if (pullback == null)
                continue;

            if (pivot.DIR.IsDown() && outSegment.DIR.IsUp())
            {
                // 下跌中枢后向上离开，回拉不进中枢（回拉低点 >= ZG） -> 三买
                if (pullback.DIR.IsDown() && pullback.Low >= pivot.ZG)
                {
                    MarkPoint(pullback.EndChanKLine.PeakUnit, isBuy: true, level: 3);
                }
            }
            else if (pivot.DIR.IsUp() && outSegment.DIR.IsDown())
            {
                // 上涨中枢后向下离开，回拉不进中枢（回拉高点 <= ZD） -> 三卖
                if (pullback.DIR.IsUp() && pullback.High <= pivot.ZD)
                {
                    MarkPoint(pullback.EndChanKLine.PeakUnit, isBuy: false, level: 3);
                }
            }
        }
    }

    /// <summary>
    /// 背驰判断：基于进入段与退出段的 MACD 面积比较
    /// </summary>
    private static bool IsDivergence(PivotBase<BiPivot, BiType> pivot, BiType outSegment)
    {
        var entry = pivot.InSegment;
        if (entry == null)
            return false;

        var entryUnits = GetBiUnits(entry);
        var outUnits = GetBiUnits(outSegment);

        if (entryUnits.Count == 0 || outUnits.Count == 0)
            return false;

        var entryMacdArea = entryUnits.Sum(u => Math.Abs((double)(u.UnitIndicator.MACD?.Histogram ?? 0)));
        var outMacdArea = outUnits.Sum(u => Math.Abs((double)(u.UnitIndicator.MACD?.Histogram ?? 0)));

        if (pivot.DIR.IsDown())
        {
            // 下跌：退出段价格更低，但 MACD 面积更小 -> 背驰（一买）
            return outSegment.Low < entry.EndChanKLine.Low && outMacdArea < entryMacdArea;
        }
        else
        {
            // 上涨：退出段价格更高，但 MACD 面积更小 -> 背驰（一卖）
            return outSegment.High > entry.EndChanKLine.High && outMacdArea < entryMacdArea;
        }
    }

    private static List<KLineUnit> GetBiUnits(BiType bi)
    {
        var units = new List<KLineUnit>();
        var curLine = bi.StartChanKLine;
        while (curLine != null && curLine.Idx <= bi.EndChanKLine.Idx)
        {
            units.AddRange(curLine.CombinedUnits);
            curLine = curLine.Next;
        }
        return units;
    }

    private static void MarkPoint(KLineUnit unit, bool isBuy, int level)
    {
        switch (level)
        {
            case 1:
                unit.UnitIndicator.IsBuy1 = isBuy ? true : null;
                unit.UnitIndicator.IsSell1 = isBuy ? null : true;
                break;
            case 2:
                unit.UnitIndicator.IsBuy2 = isBuy ? true : null;
                unit.UnitIndicator.IsSell2 = isBuy ? null : true;
                break;
            case 3:
                unit.UnitIndicator.IsBuy3 = isBuy ? true : null;
                unit.UnitIndicator.IsSell3 = isBuy ? null : true;
                break;
        }
    }
}
