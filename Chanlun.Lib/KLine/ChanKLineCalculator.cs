using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.KLine;

public class ChanKLineCalculator
{
    public static void Calculate(int nCount, float[] pHigh, float[] pLow, float[] pKey, ref ChanCalculateResult result)
    {
        float key = pKey[0];
        var times = StockTimeCache.Get(key);

        if (times.IsNullOrEmpty() || times.Count != nCount)
        {
            return;
        }
        
        var lineList = new ChanKLineList();
        for (int i = 0; i < nCount; i++)
        {
            // 合并 k 线
            lineList.CreateOrUpdateKLineCombineFromUnit(new KLineUnit(i)
            {
                High = pHigh[i],
                Low = pLow[i],
                Time = times[i],
            });
            
        }

        result.LineList = lineList;
    }
}