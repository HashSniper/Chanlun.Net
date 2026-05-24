using Chanlun.Lib.Bi;
using Chanlun.Lib.Memory;
using Chanlun.Lib.SEG;

namespace Chanlun.Lib;

public class ChanCalculator
{
  

    public ChanCalculateResult Calculate(int nCount,float[] pHigh, float[] pLow, float[] pkey)
    {
        float key = pkey[0];
        
        ChanCalculateResult result = new ChanCalculateResult();
        result.BiList = BiCalculator.Calculate(nCount, pHigh, pLow, pkey);
        result.SegList = SegCalculator.Calculate(key, result.BiList);
        
        ChanCalculateResultCache.Add(key,result);
        return result;
    }
}