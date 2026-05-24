using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.Bi;

public static class BiCalculator
{
    public static BiList Calculate(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
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
            biList.CreateOrUpdateBiFromKLine(kLine, countBefore < kLineCombines.Count);
        }

        return biList;
        

        
        // // 将笔 组装成 output
        // foreach (var bi in biList)
        // {
        //     if (bi.DIR == ChanDir.UP)
        //     {
        //         pOut[bi.EndKLine.PeakUnit.Idx] = 1;
        //         pOut[bi.StartKLine.PeakUnit.Idx] = -1;
        //     }
        //     else
        //     {
        //         pOut[bi.EndKLine.PeakUnit.Idx] = -1;
        //         pOut[bi.StartKLine.PeakUnit.Idx] = 1;
        //     }
        // }
        //
        // return pOut;
    }
}
