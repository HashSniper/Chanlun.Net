using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.Bi;

public static class BiCalculator
{
    public static void Calculate(ref ChanCalculateResult result)
    {
        var lineList = result.LineList;
        var biList = new BiList();
        foreach (var line in lineList)
        {
            biList.CreateOrUpdateBiFromKLine(line);
        }
        result.BiList = biList;
    }
    
    public static float[] GetBi(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
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
