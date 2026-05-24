using ChanAdapter.Memory;

namespace ChanAdapter;

public static class SegAdapter
{
    public static float[] GetSeg(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var kLineList = KLineListCache.Get(key);
        var pOut = new float[nCount];
        if (kLineList == null)
        {
            return pOut;
        }

        var segList = kLineList.SegList;
        foreach (var seg in segList)
        {
            if (seg.IsUp())
            {
                pOut[seg.EndChan.EndKlc.GetPeakKlu(true).Idx] = 1;
                pOut[seg.StartChan.BeginKlc.GetPeakKlu(false).Idx] = -1;
            }
            else if(seg.IsDown())
            {
                pOut[seg.EndChan.EndKlc.GetPeakKlu(false).Idx] = -1;
                pOut[seg.StartChan.BeginKlc.GetPeakKlu(true).Idx] = 1;
            }
        }
        return pOut;
    }
}