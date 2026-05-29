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

        // var segList = calculateResult.SegList;
        // if (segList.IsNullOrEmpty())
        // {
        //     return pOut;
        // }
        //
        // foreach (var seg in segList)
        // {
        //     float amount = 0;
        //     var curKline = seg.StartBi.StartChanKLine;
        //     while (curKline != null && curKline.Idx <= seg.EndBi.EndChanKLine.Idx)
        //     {
        //         amount += curKline.CombinedUnits
        //             .Where(p => seg.DIR.IsDown() ? p.MACD.Hist < 0 : p.MACD.Hist > 0)
        //             .Sum(p => p.MACD.Hist);
        //         curKline = curKline.Next;
        //     }
        //
        //     pOut[seg.EndBi.EndChanKLine.PeakUnit.Idx] = MathF.Round(amount, 2);
        // }
        
        var biList = calculateResult.BiList;
        if (biList.IsNullOrEmpty())
        {
            return pOut;
        }

        foreach (var bi in biList)
        {
            float amount = 0;
            var curKline = bi.StartChanKLine;
            while (curKline != null && curKline.Idx <= bi.EndChanKLine.Idx)
            {
                amount += curKline.CombinedUnits
                    .Where(p => bi.DIR.IsDown() ? p.MACD.Hist < 0 : p.MACD.Hist > 0)
                    .Sum(p => p.MACD.Hist);
                curKline = curKline.Next;
            }

            pOut[bi.EndChanKLine.PeakUnit.Idx] = MathF.Round(amount, 2);
        }

#if DEBUG
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "macd_output.txt");
        using (var writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine($"key: {key}");
            for (int i = 0; i < pOut.Length; i++)
            {
                if (pOut[i] != 0)
                {
                    writer.WriteLine($"{i}: {pOut[i]}");
                }
            }
        }
#endif

        return pOut;
    }
}