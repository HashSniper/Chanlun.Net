using Chanlun.Lib;
using Chanlun.Lib.Bi;
using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Memory;
using Chanlun.Lib.SEG;
using Chanlun.Lib.Zs;
using Skender.Stock.Indicators;

namespace Chanlun.API.Adapter;

public static class ChanCalculator
{
    public static void Calculate(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pkey)
    {
        decimal key = pkey[0];
        var result = ChanCalculateResultCache.Get(key);

        ChanKLineCalculator.Calculate(nCount, pHigh, pLow, pkey, ref result);
        BiCalculator.Calculate(ref result);
        SegCalculator.Calculate(ref result);
        PivotCalculator.Calculate(ref result);

        ChanCalculateResultCache.Add(key, result);
    }
}

public static class BiCalculator
{
    public static void Calculate(ref ChanCalculateResult result)
    {
        var lineList = result.LineList;
        var biList = new BiList();
        List<ChanKLine> fxs = new List<ChanKLine>();
        foreach (var line in lineList)
        {
            if (line.FX != ChanFX.UNKNOWN)
            {
                if (fxs.Count == 0 || fxs.Last().FX != line.FX)
                {
                    fxs.Add(line);
                }
                else if ((fxs.Last().FX == ChanFX.BOTTOM && line.Low < fxs.Last().Low) ||
                         (fxs.Last().FX == ChanFX.TOP && line.High > fxs.Last().High))
                {
                    fxs.RemoveEnd();
                    fxs.Add(line);
                }
            }
        }

        foreach (var fx in fxs)
        {
            biList.CreateOrUpdateBiFromKLine(fx);
        }

        result.BiList = biList;
    }
    
    public static decimal[] GetBi(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var biList = calculateResult.BiList;

        foreach (var bi in biList)
        {
            if (bi.DIR.IsUp())
            {
                pOut[bi.EndChanKLine.PeakUnit.Idx] = 1;
                pOut[bi.StartChanKLine.PeakUnit.Idx] = -1;
            }
            else
            {
                pOut[bi.EndChanKLine.PeakUnit.Idx] = -1;
                pOut[bi.StartChanKLine.PeakUnit.Idx] = 1;
            }
        }
        return pOut;
    }
}


public static class ChanKLineCalculator
{
    public static void Calculate(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey, ref ChanCalculateResult result)
    {
        KLineDataPopulator.PopulateHighLowPrice(nCount, pHigh, pLow, pKey);

        var unitList = result.UnitList;
        var lineList = new ChanKLineList();

        foreach (var unit in unitList)
        {
            lineList.CreateOrUpdateKLineCombineFromUnit(unit);
        }
        
        result.LineList = lineList;
    }

    public static decimal[] GetKLineG(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var kList = calculateResult.LineList;

        foreach (var kLine in kList)
        {
            if (kLine.CombinedUnits.Count == 1)
            {
                continue;
            }

            foreach (var unit in kLine.CombinedUnits)
            {
                pOut[unit.Idx] = kLine.High;
            }
        }

        return pOut;
    }

    public static decimal[] GetKLineD(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var kList = calculateResult.LineList;

        foreach (var kLine in kList)
        {
            if (kLine.CombinedUnits.Count == 1)
            {
                continue;
            }

            foreach (var unit in kLine.CombinedUnits)
            {
                pOut[unit.Idx] = kLine.Low;
            }
        }

        return pOut;
    }

    public static decimal[] GetKLineRange(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var kList = calculateResult.LineList;

        foreach (var kLine in kList)
        {
            if (kLine.CombinedUnits.Count == 1)
            {
                continue;
            }

            // 合并区间内的每根K线都标记为1，形成连续的矩形框
            foreach (var unit in kLine.CombinedUnits)
            {
                pOut[unit.Idx] = 1;
            }
        }

        return pOut;
    }
}

public static class SegCalculator
{
    public static void Calculate(ref ChanCalculateResult result)
    {
        var segs = new SegList();
        var bis = result.BiList;
        if (bis.IsNullOrEmpty())
        {
            return;
        }

        segs.CreateOrUpdateSeg(bis);

        result.SegList = segs;
    }


    public static decimal[] GetSegs(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        var segList = calculateResult?.SegList;
        if (segList == null)
        {
            return pOut;
        }

      
        foreach (var seg in segList)
        {
            if (seg.DIR.IsUp())
            {
                pOut[seg.EndBi.EndChanKLine.PeakUnit.Idx] = 1;
                pOut[seg.StartBi.StartChanKLine.PeakUnit.Idx] = -1;
            }
            else if (seg.DIR.IsDown())
            {
                pOut[seg.EndBi.EndChanKLine.PeakUnit.Idx] = -1;
                pOut[seg.StartBi.StartChanKLine.PeakUnit.Idx] = 1;
            }
        }
#if DEBUG
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seg_output.txt");
        using (var writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine($"key: {key}");
            for (int i = 0; i < pOut.Length; i++)
            {
                if (pOut[i] == 1 || pOut[i] == -1)
                {
                    writer.WriteLine($"{i}: {pOut[i]}");
                }
            }
        }
#endif
        
        return pOut;
    }
}


public static class PivotCalculator
{
    public static void Calculate(ref ChanCalculateResult result)
    {
        var pivotList = new SegPivotList();
        pivotList.CreateOrUpdatePivot(result.SegList);
        result.SegPivotList = pivotList;
        
        var biPivotList = new BiPivotList();
        biPivotList.CreateOrUpdatePivot(result.BiList);
        result.BiPivotList = biPivotList;
    }
    
    
    public static decimal[] GetSegPivotZG(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.SegPivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segments[0].StartBi.StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segments[^1].EndBi.EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[i] = zs.ZG;
            }
        }

        return pOut;
    }
    
    public static decimal[] GetSegPivotZD(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.SegPivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segments[0].StartBi.StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segments[^1].EndBi.EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[i] = zs.ZD;
            }
        }

        return pOut;
    }
    
    public static decimal[] GetSegPivotRange(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.SegPivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segments[0].StartBi.StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segments[^1].EndBi.EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[startIdx] = 1;
                pOut[endIdx] = 2;
            }
        }
        return pOut;
    }
    
    
    public static decimal[] GetBiPivotZG(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.BiPivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segments[0].StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segments[^1].EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[i] = zs.ZG;
            }
        }

        return pOut;
    }
    
    public static decimal[] GetBiPivotZD(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.BiPivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segments[0].StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segments[^1].EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[i] = zs.ZD;
            }
        }

        return pOut;
    }
    
    public static decimal[] GetBiPivotRange(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.BiPivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segments[0].StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segments[^1].EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[startIdx] = 1;
                pOut[endIdx] = 2;
            }
        }
        return pOut;
    }
}


public static class IndicatorCalculator
{
    public static decimal[] Calculate(int nCount, decimal[] pOpen, decimal[] pClose, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var kLines = calculateResult.LineList;
        if (kLines.IsNullOrEmpty())
        {
            return pOut;
        }

        KLineDataPopulator.PopulateOpenClosePrice(nCount, pOpen, pClose, pKey);
        var units = calculateResult.UnitList;

        var bars = units.ConvertToBars();
        var macds = bars.GetMacd().ToList();
        var bollList = bars.GetBollingerBands().ToList();
        foreach (var unit in units)
        {
            unit.MACD = macds[unit.Idx];
            unit.Boll = bollList[unit.Idx];
        }

        var biList = calculateResult.BiList;
        if (biList.IsNullOrEmpty())
        {
            return pOut;
        }


        foreach (var bi in biList)
        {
            var energy = CalculateBiMetric1(bi);
            pOut[bi.EndChanKLine.PeakUnit.Idx] = energy;
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
            // 1. 获取 MACD 柱值（乘上乘数以匹配国内习惯）
            double macdHist = (double)(k.MACD.Histogram ?? 0) * macdMultiplier;

            // 2. 计算布林带 %b
            double upper = (double)(k.Boll.UpperBand ?? 0);
            double lower = (double)(k.Boll.LowerBand ?? 0);
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
        double macdArea = units.Where(p => bi.DIR.IsDown() ? p.MACD.Histogram < 0 : p.MACD.Histogram > 0)
            .Sum(b => b.MACD.Histogram ?? 0);

        // 2. 计算 BOLL 空间拓张率
        double bollExtension = 0;

        if (bi.DIR.IsUp())
        {
            // 找到最高点所在的那根 K 线
            var maxBar = units.MaxBy(b => b.High);
            double denominator = (maxBar.Boll.UpperBand ?? 0) - (maxBar.Boll.Sma ?? 0);

            if (denominator > 0)
            {
                bollExtension = ((double)maxBar.High - (maxBar.Boll.Sma ?? 0)) / denominator;
            }
        }
        else // Down
        {
            // 找到最低点所在的那根 K 线
            var minBar = units.MinBy(b => b.Low);
            double denominator = (minBar.Boll.Sma ?? 0) - (minBar.Boll.LowerBand ?? 0);

            if (denominator > 0)
            {
                bollExtension = ((minBar.Boll.Sma ?? 0) - (double)minBar.Low) / denominator;
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

public static class KLineDataPopulator
{
    private static decimal BuildBaseKey(string code, decimal[] date, decimal[] time)
    {
        var keyString = $"{code}_{date[0]}:{time[0]}-{date[^1]}:{time[^1]}_{date.Length}";
        // FNV-1a 哈希，取低24位确保在 decimal 精确整数范围内（2^24 = 16,777,216）
        uint hash = 2166136261u;
        foreach (char c in keyString)
        {
            hash ^= c;
            hash *= 16777619u;
        }
        return hash & 0x00FFFFFF;
    }
    /// <summary>
    /// 必须在第一步执行
    /// </summary>
    /// <param name="nCount"></param>
    /// <param name="code"></param>
    /// <param name="date"></param>
    /// <param name="time"></param>
    public static decimal[] PopulateTradeTime(int nCount, decimal[] code, decimal[] date, decimal[] time)
    {
        int codeValue = (int)code[0];
        string stockCode = codeValue.ToString("D6");
        
        var baseKey = BuildBaseKey(stockCode, date, time);
        var result = new ChanCalculateResult
        {
            Symbol = stockCode
        };
        var unitList = new List<KLineUnit>();
        for (int i = 0; i < date.Length; i++)
        {
            int d = (int)date[i];
            int t = (int)time[i];
            
            unitList.Add(new KLineUnit(i)
            {
                Time = Utils.ConvertToDateTime(d, t)
            });
        }

        result.UnitList = unitList;
        
        var pOut = new decimal[date.Length];
        pOut[0] = baseKey;
        for (int i = 1; i < nCount; ++i)
        {
            pOut[i] = i;
        }
        ChanCalculateResultCache.Add(baseKey, result);
        
        return pOut;
    }

    public static void PopulateHighLowPrice(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult?.UnitList == null)
        {
            return;
        }

        for (int i = 0; i < nCount; ++i)
        {
            var kLineUnit = calculateResult.UnitList[i];
            kLineUnit.High = pHigh[i];
            kLineUnit.Low = pLow[i];
        }
    }

    /// <summary>
    /// 处理k 线的最后一步
    /// </summary>
    /// <param name="nCount"></param>
    /// <param name="pVolume"></param>
    /// <param name="pAmount"></param>
    /// <param name="pKey"></param>
    public static void PopulateVolume(int nCount, decimal[] pVolume, decimal[] pAmount, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult?.UnitList == null)
        {
            return;
        }

        for (int i = 0; i < nCount; ++i)
        {
            var kLineUnit = calculateResult.UnitList[i];
            kLineUnit.Amount = pAmount[i];
            kLineUnit.Volume = pVolume[i];
        }

    }
    
    public static void PopulateOpenClosePrice(int nCount, decimal[] pOpen, decimal[] pClose, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult?.UnitList == null)
        {
            return;
        }

        for (int i = 0; i < nCount; ++i)
        {
            var kLineUnit = calculateResult.UnitList[i];
            kLineUnit.Open = pOpen[i];
            kLineUnit.Close = pClose[i];
        }
    }
}