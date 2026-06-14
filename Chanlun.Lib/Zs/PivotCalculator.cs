using Chanlun.Lib;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.Zs;

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
