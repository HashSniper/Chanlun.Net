using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.Zs;

public static class PivotCalculator
{
    public static void Calculate(ref ChanCalculateResult result)
    {
        var pivotList = new PivotList();
        pivotList.CreateOrUpdatePivot(result.SegList);
        result.PivotList = pivotList;
    }
    
    
    public static float[] GetPivotZG(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.PivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segs[0].StartBi.StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segs[^1].EndBi.EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[i] = zs.ZG;
            }
        }

        return pOut;
    }
    
    public static float[] GetPivotZD(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.PivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segs[0].StartBi.StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segs[^1].EndBi.EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[i] = zs.ZD;
            }
        }

        return pOut;
    }
    
    public static float[] GetPivotRange(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var pivotList = calculateResult.PivotList;
        foreach (var zs in pivotList)
        {
            var startIdx = zs.Segs[0].StartBi.StartChanKLine.PeakUnit.Idx;
            var endIdx = zs.Segs[^1].EndBi.EndChanKLine.PeakUnit.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[startIdx] = 1;
                pOut[endIdx] = 2;
            }
        }
        return pOut;
    }
}