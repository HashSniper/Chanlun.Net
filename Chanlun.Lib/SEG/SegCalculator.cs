using Chanlun.Lib;
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


    public static decimal[] GetSegs(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new decimal[nCount];
        var segList = calculateResult?.SegList;
        if (segList == null)
        {
            return pOut;
        }

      
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
#if DEBUG
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seg_output.txt");
        using (var writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine($"key: {key}");
            for (int i = 0; i < pOut.Length; i++)
            {
                if (pOut[i] == 1 || pOut[i] == -1)
                {
                    writer.WriteLine($"{i}: {pOut[i]}");
                }
            }
        }
#endif
        
        return pOut;
    }
}
