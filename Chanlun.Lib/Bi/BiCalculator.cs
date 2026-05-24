using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Memory;
using Chanlun.Lib.SEG;

namespace Chanlun.Lib.Bi;

public static class BiCalculator
{
    public static void Calculate(int nCount, float[] pHigh, float[] pLow, float[] pKey, ref ChanCalculateResult result)
    {
        float key = pKey[0];
        var times = StockTimeCache.Get(key);

        if (times.IsNullOrEmpty() || times.Count != nCount)
        {
            return;
        }

        var kLineCombines = new KLineGroupList();
        var biList = new BiList();
        for (int i = 0; i < nCount; i++)
        {
            var countBefore = kLineCombines.Count;
            // 合并 k 线
            var kLine = kLineCombines.CreateOrUpdateKLineCombineFromUnit(new KLineUnit(i)
            {
                High = pHigh[i],
                Low = pLow[i],
                Time = times[i],
            });
            biList.CreateOrUpdateBiFromKLine(kLine, countBefore < kLineCombines.Count);

            
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
                pOut[bi.EndKLine.PeakUnit.Idx] = 1;
                pOut[bi.StartKLine.PeakUnit.Idx] = -1;
            }
            else
            {
                pOut[bi.EndKLine.PeakUnit.Idx] = -1;
                pOut[bi.StartKLine.PeakUnit.Idx] = 1;
            }
        }
        return pOut;
    }
}
