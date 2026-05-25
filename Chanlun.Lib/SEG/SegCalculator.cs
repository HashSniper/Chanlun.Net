using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.SEG;

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
    
    
    public static float[] GetSegs(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var segList = calculateResult.SegList;
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

        return pOut;
    }
}