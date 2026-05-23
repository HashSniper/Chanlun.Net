using Chanlun.Lib.Bi;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Memory;

public static class KLineUnitCache
{
    public static void Add(float key, List<KLineUnit> value)
    {
        ChanMemory.Add(key,nameof(KLineUnitCache), value);
    }

    public static List<KLineUnit>? Get(float key)
    {
        return  ChanMemory.Get(key,nameof(KLineUnitCache)) as List<KLineUnit>;
    }
}