using Chan.Lib.Common;
using ChanAdapter.Memory;

namespace ChanAdapter;

public static class BiAdapter
{
    public static float[] GetBi(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var kLineList = KLineListCache.Get(key);
        var pOut = new float[nCount];
        if (kLineList == null)
        {
            return pOut;
        }

        var biList = kLineList.BiList;

        foreach (var bi in biList)
        {
            if (bi.IsUp())
            {
                pOut[bi.EndKlc.GetPeakKlu(true).Idx] = 1;
                pOut[bi.BeginKlc.GetPeakKlu(false).Idx] = -1;
            }
            else if(bi.IsDown())
            {
                pOut[bi.EndKlc.GetPeakKlu(false).Idx] = -1;
                pOut[bi.BeginKlc.GetPeakKlu(true).Idx] = 1;
            }
        }
        return pOut;
    }
}