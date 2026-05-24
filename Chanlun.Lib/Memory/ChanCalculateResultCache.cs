using Chanlun.Lib.Bi;

namespace Chanlun.Lib.Memory;

public static class ChanCalculateResultCache
{
    public static void Add(float key, ChanCalculateResult value)
    {
        ChanMemory.Add(key,nameof(ChanCalculateResultCache), value);
    }

    public static ChanCalculateResult? Get(float key)
    {
        return  ChanMemory.Get<ChanCalculateResult>(key,nameof(ChanCalculateResultCache));
    }
}