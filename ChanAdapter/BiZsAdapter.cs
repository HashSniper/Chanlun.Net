using ChanAdapter.Memory;

namespace ChanAdapter;

public static class BiZsAdapter
{
    /// <summary>
    /// 获取笔中枢高点
    /// </summary>
    /// <param name="nCount"></param>
    /// <param name="pHigh"></param>
    /// <param name="pLow"></param>
    /// <param name="pKey"></param>
    /// <returns></returns>
    public static float[] GetBiZSZG(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var kLineList = KLineListCache.Get(key);
        var pOut = new float[nCount];
        if (kLineList == null)
        {
            return pOut;
        }

        var biZs = kLineList.ZsList;
        foreach (var zs in biZs)
        {
            var startIdx = zs.Begin.Idx;
            var endIdx = zs.End.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[i] = zs.High;
            }

        }

        return pOut;
    }
    
    public static float[] GetBiZSZD(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var kLineList = KLineListCache.Get(key);
        var pOut = new float[nCount];
        if (kLineList == null)
        {
            return pOut;
        }

        var biZs = kLineList.ZsList;
        foreach (var zs in biZs)
        {
            var startIdx = zs.Begin.Idx;
            var endIdx = zs.End.Idx;
            for (var i = startIdx; i <= endIdx; i++)
            {
                pOut[i] = zs.Low;
            }
        }
        return pOut;
    }
    
    public static float[] GetBiZSRange(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var kLineList = KLineListCache.Get(key);
        var pOut = new float[nCount];
        if (kLineList == null)
        {
            return pOut;
        }

        var biZs = kLineList.ZsList;

        foreach (var zs in biZs)
        {
            var startIdx = zs.Begin.Idx;
            var endIdx = zs.End.Idx;
            pOut[startIdx] = 1;
            pOut[endIdx] = 2;
        }
        return pOut;
    }
}