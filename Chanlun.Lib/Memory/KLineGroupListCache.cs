using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Memory;

public static class KLineGroupListCache
{
    public static void Add(float key, KLineGroupList value)
    {
        ChanMemory.Add(key,nameof(KLineGroupListCache), value);
    }

    public static KLineGroupList? Get(float key)
    {
        return  ChanMemory.Get(key,nameof(KLineGroupListCache)) as KLineGroupList;
    }
}