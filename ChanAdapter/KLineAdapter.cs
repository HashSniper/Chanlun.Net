using ChanAdapter.Memory;

namespace ChanAdapter;

public static class KLineAdapter
{
    public static float[] GetKLineG(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var kLineList = KLineListCache.Get(key);
        var pOut = new float[nCount];
        if (kLineList == null)
        {
            return pOut;
        }

        var kList = kLineList.Lst;
       
        foreach (var kLine in kList)
        {
            if (kLine.Lst.Count == 1)
            {
                continue;
            }
            foreach (var unit in  kLine.Lst)
            {
                pOut[unit.Idx] = kLine.High;
            }
        }

        return pOut;
    }
    
    public static float[] GetKLineD(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var kLineList = KLineListCache.Get(key);
        var pOut = new float[nCount];
        if (kLineList == null)
        {
            return pOut;
        }

        var kList = kLineList.Lst;
       
        foreach (var kLine in kList)
        {
            if (kLine.Lst.Count == 1)
            {
                continue;
            }

            foreach (var unit in  kLine.Lst)
            {
                pOut[unit.Idx] = kLine.Low;
            }
        }
        return pOut;
    }
    
    public static float[] GetKLineRange(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var kLineList = KLineListCache.Get(key);
        var pOut = new float[nCount];
        if (kLineList == null)
        {
            return pOut;
        }
        
        var kList = kLineList.Lst;
       
        foreach (var kLine in kList)
        {
            if (kLine.Lst.Count == 1)
            {
                continue;
            }
            
            var startIdx = kLine.Lst[0].Idx;
            var endIdx = kLine.Lst.Last().Idx;
            pOut[startIdx] = 1;
            pOut[endIdx] = 2;
        }
        return pOut;
    }
}