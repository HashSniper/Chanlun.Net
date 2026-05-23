using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Memory;
using Chanlun.Lib.ZS;

namespace Chanlun.Lib.Bi;

public static class BiCalculator
{
    public static float[] Bi(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        float[] pOut = new float[nCount];
        float key = pKey[0];
        var times = StockTimeCache.Get(key);

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
            var hasCreateOrUpdateBi = biList.CreateOrUpdateBiFromKLine(kLine, countBefore < kLineCombines.Count);
            if (hasCreateOrUpdateBi)
            {
                BiZSCalculator.BiZS(key, biList);
            }
        }
        
        KLineGroupListCache.Add(key, kLineCombines);
        BiListCache.Add(key, biList);
        
        // 将笔 组装成 output
        foreach (var bi in biList)
        {
            if (bi.DIR == ChanDir.UP)
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

    public static float[] GetKLineCombineIndex(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        float[] pOut = new float[nCount];
        float key = pKey[0];
        var kLines = KLineGroupListCache.Get(key);
        foreach (var kLine in kLines)
        {
            foreach (var unit in kLine.CombinedUnits)
            {
                pOut[unit.Idx] = kLine.Idx;
            }
        }

        return pOut;
    }
}
