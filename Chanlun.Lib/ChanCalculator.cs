using Chanlun.Lib.Bi;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Memory;
using Chanlun.Lib.SEG;
using Chanlun.Lib.Zs;

namespace Chanlun.Lib;

public static class ChanCalculator
{
    public static void Calculate(int nCount, float[] pHigh, float[] pLow, float[] pkey)
    {
        float key = pkey[0];
        var result = new ChanCalculateResult();

        ChanKLineCalculator.Calculate(nCount, pHigh, pLow, pkey, ref result);
        BiCalculator.Calculate(ref result);
        SegCalculator.Calculate(ref result);
        PivotCalculator.Calculate(ref result);

        ChanCalculateResultCache.Add(key, result);
    }
}