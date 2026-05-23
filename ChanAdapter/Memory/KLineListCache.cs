
using Chan.Lib.Bis;
using Chan.Lib.KLines;

namespace ChanAdapter.Memory;

public static class KLineListCache
{
    public static void Add(float key, KLineList value)
    {
        ChanMemory.Add(key,nameof(KLineListCache), value);
    }

    public static KLineList? Get(float key)
    {
        return  ChanMemory.Get(key,nameof(KLineListCache)) as KLineList;
    }
}