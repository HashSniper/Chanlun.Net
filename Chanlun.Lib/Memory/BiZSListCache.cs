using Chanlun.Lib.ZS;

namespace Chanlun.Lib.Memory;

public static class BiZSListCache
{
    public static void Add(float key, BiZSList value)
    {
        ChanMemory.Add(key,nameof(BiZSListCache), value);
    }

    public static BiZSList? Get(float key)
    {
        return ChanMemory.Get(key,nameof(BiZSListCache)) as BiZSList;
    }
}