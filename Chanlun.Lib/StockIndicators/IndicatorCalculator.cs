using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.StockIndicators;

public static class IndicatorCalculator
{
    public static decimal[] Calculate(ref ChanCalculateResult calculateResult)
    {
        if (calculateResult == null)
        {
            return [];
        }
        var pOut = new decimal[calculateResult.UnitList.Count];

        var kLines = calculateResult.LineList;
        if (kLines.IsNullOrEmpty())
        {
            return pOut;
        }
        
        var units = calculateResult.UnitList;
        
        var biList = calculateResult.BiList;
        if (biList.IsNullOrEmpty())
        {
            return pOut;
        }
        
        foreach (var bi in biList)
        {
            var energy = CalculateBiMetric1(bi);
            pOut[bi.EndChanKLine.PeakUnit.Idx] = energy;
            units[bi.EndChanKLine.PeakUnit.Idx].UnitIndicator.ChanEnergy = energy;
        }

        return pOut;
    }

    private static decimal CalculateBiMetric1(Lib.Bi.Bi bi)
    {
        double energy = 0.0;
        double macdMultiplier = 2.0;
        
        var units = new List<KLineUnit>();
        var curLine = bi.StartChanKLine;
        while (curLine != null && curLine.Idx <= bi.EndChanKLine.Idx)
        {
            units.AddRange(curLine.CombinedUnits);
            curLine = curLine.Next;
        }

        foreach (var k in units)
        {
            if(k.UnitIndicator.MACD==null||k.UnitIndicator.Boll==null)
            {
                continue;
            }
            // 1. 获取 MACD 柱值（乘上乘数以匹配国内习惯）
            double macdHist = (double)(k.UnitIndicator.MACD.Histogram ?? 0) * macdMultiplier;

            // 2. 计算布林带 %b
            double upper = (double)(k.UnitIndicator.Boll.UpperBand ?? 0);
            double lower = (double)(k.UnitIndicator.Boll.LowerBand ?? 0);
            double percentB;

            if (Math.Abs(upper - lower) < 1e-12)
                percentB = 0.5; // 异常情况的兜底
            else
                percentB = ((double)k.Close - lower) / (upper - lower);

            // 3. 根据笔方向累加能量
            if (bi.DIR.IsUp())
            {
                // 向上笔：红柱与高 %b 贡献大，绿柱或低 %b 削弱或忽略
                double weight = Math.Max(percentB, 0);
                energy += macdHist * weight;
            }
            else // BiDirection.Down
            {
                // 向下笔：绿柱（-macdHist > 0）配合低 %b 贡献大，红柱或高 %b 削弱或忽略
                double weight = Math.Max(1.0 - percentB, 0);
                energy += macdHist * weight;   // 绿柱时 -macdHist 为正
            }
        }

        return (decimal)energy;
        
        // if (firstBi.Direction == BiDirection.Up)
        // {
        //     // 顶背驰：B 高点更高，但能量更小
        //     return secondBi.ExtremePrice > firstBi.ExtremePrice &&
        //            secondBi.Energy < firstBi.Energy;
        // }
        // else
        // {
        //     // 底背驰：B 低点更低，但能量更小
        //     return secondBi.ExtremePrice < firstBi.ExtremePrice &&
        //            secondBi.Energy < firstBi.Energy;
        // }
        
    }

    private static decimal CalculateBiMetric(Lib.Bi.Bi bi)
    {
        var units = new List<KLineUnit>();
        var curLine = bi.StartChanKLine;
        while (curLine != null && curLine.Idx <= bi.EndChanKLine.Idx)
        {
            units.AddRange(curLine.CombinedUnits);
            curLine = curLine.Next;
        }

        // 1. 计算 MACD 动能总量 (面积积分)
        double macdArea = units.Where(p => bi.DIR.IsDown() ? p.UnitIndicator.MACD.Histogram < 0 : p.UnitIndicator.MACD.Histogram > 0)
            .Sum(b => b.UnitIndicator.MACD.Histogram ?? 0);

        // 2. 计算 BOLL 空间拓张率
        double bollExtension = 0;

        if (bi.DIR.IsUp())
        {
            // 找到最高点所在的那根 K 线
            var maxBar = units.MaxBy(b => b.High);
            double denominator = (maxBar.UnitIndicator.Boll.UpperBand ?? 0) - (maxBar.UnitIndicator.Boll.Sma ?? 0);

            if (denominator > 0)
            {
                bollExtension = ((double)maxBar.High - (maxBar.UnitIndicator.Boll.Sma ?? 0)) / denominator;
            }
        }
        else // Down
        {
            // 找到最低点所在的那根 K 线
            var minBar = units.MinBy(b => b.Low);
            double denominator = (minBar.UnitIndicator.Boll.Sma ?? 0) - (minBar.UnitIndicator.Boll.LowerBand ?? 0);

            if (denominator > 0)
            {
                bollExtension = ((minBar.UnitIndicator.Boll.Sma ?? 0) - (double)minBar.Low) / denominator;
            }
        }

        // 3. 乘积得出单笔最终值 (若算出力度为负，强制归 0)
        return (decimal)(macdArea * bollExtension);
    }

    // // <summary>
    // /// 终点步骤：直接对两笔算出来的值进行链式比对，输出背驰判定
    // /// </summary>
    // public bool IsDivergence(Pen p_n_minus_2, Pen p_n)
    // {
    //     // 在比较前，确保两笔各自的值已经通过公式计算完成
    //     CalculatePenMetric(p_n_minus_2);
    //     CalculatePenMetric(p_n);
    //
    //     if (p_n.Direction == PenDirection.Up)
    //     {
    //         // 顶背驰条件：价格创新高，但 PMSV 综合指标值反而变小
    //         return p_n.PeakPrice > p_n_minus_2.PeakPrice && p_n.PMSV < p_n_minus_2.PMSV;
    //     }
    //     else
    //     {
    //         // 底背驰条件：价格创新低，但 PMSV 综合指标值反而变小
    //         return p_n.PeakPrice < p_n_minus_2.PeakPrice && p_n.PMSV < p_n_minus_2.PMSV;
    //     }
    // }
}
