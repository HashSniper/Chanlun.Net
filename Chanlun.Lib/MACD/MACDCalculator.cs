using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.MACD;

public static class MACDCalculator
{
    public static float[] Calculate(int nCount, float[] pDif, float[] pDea, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var kLines = calculateResult.LineList;
        if (kLines.IsNullOrEmpty())
        {
            return pOut;
        }

        foreach (var kLine in kLines)
        {
            foreach (var unit in kLine.CombinedUnits)
            {
                unit.MACD = new MACD(pDif[unit.Idx], pDea[unit.Idx]);
            }
        }
        
        var segList = calculateResult.SegList;
        if (segList.IsNullOrEmpty())
        {
            return pOut;
        }

        foreach (var seg in segList)
        {
            float amount = 0;
            var curBi = seg.StartBi;
            while (curBi!=null && curBi.Idx <= seg.EndBi.Idx)
            {
                var curKline = curBi.StartChanKLine;
                while (curKline!=null&& curKline.Idx <= curBi.EndChanKLine.Idx)
                {
                    amount += curKline.CombinedUnits.Sum(p => p.MACD.MACDHist);
                    curKline = curKline.Next;
                }

                curBi = curBi.Next;
            }

            pOut[seg.EndBi.EndChanKLine.PeakUnit.Idx] = amount;
        }

        return pOut;
    }
}