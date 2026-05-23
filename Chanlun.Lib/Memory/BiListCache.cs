using Chanlun.Lib.Bi;

namespace Chanlun.Lib.Memory;

public static class BiListCache
{
    public static void Add(float key, BiList value)
    {
        ChanMemory.Add(key,nameof(BiListCache), value);
    }

    public static BiList? Get(float key)
    {
        return  ChanMemory.Get(key,nameof(BiListCache)) as BiList;
    }
}