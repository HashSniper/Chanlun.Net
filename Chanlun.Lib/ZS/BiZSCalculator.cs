using Chanlun.Lib.Bi;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.ZS;

public static class BiZSCalculator
{
    public static BiZSList BiZS(float key, BiList biList)
    {
        BiZSList biZsList = [];
        
        
        BiZSListCache.Add(key, biZsList);
        return biZsList;
    }
}