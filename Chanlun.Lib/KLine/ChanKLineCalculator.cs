using Chanlun.Lib;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.KLine;

public static class ChanKLineCalculator
{
    public static void Calculate(ref ChanCalculateResult result)
    {
        
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
