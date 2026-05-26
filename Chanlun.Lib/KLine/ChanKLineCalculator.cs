using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.KLine;

public static class ChanKLineCalculator
{
    public static void Calculate(int nCount, float[] pHigh, float[] pLow, float[] pKey, ref ChanCalculateResult result)
    {
        float key = pKey[0];
        var times = StockTimeCache.Get(key);

        if (times.IsNullOrEmpty() || times.Count != nCount)
        {
            return;
        }
        
        var lineList = new ChanKLineList();
        for (int i = 0; i < nCount; i++)
        {
            // 合并 k 线
            lineList.CreateOrUpdateKLineCombineFromUnit(new KLineUnit(i)
            {
                High = pHigh[i],
                Low = pLow[i],
                Time = times[i],
            });
            
        }

        result.LineList = lineList;
    }
    public static float[] GetKLineG(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
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
            foreach (var unit in  kLine.CombinedUnits)
            {
                pOut[unit.Idx] = kLine.High;
            }
        }

        return pOut;
    }
    
    public static float[] GetKLineD(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
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
            foreach (var unit in  kLine.CombinedUnits)
            {
                pOut[unit.Idx] = kLine.Low;
            }
        }

        return pOut;
    }
    
    public static float[] GetKLineRange(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
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
            
            var startIdx = kLine.CombinedUnits[0].Idx;
            var endIdx = kLine.CombinedUnits.Last().Idx;
            pOut[startIdx] = 1;
            pOut[endIdx] = 2;
        }
        return pOut;
    }
    
}