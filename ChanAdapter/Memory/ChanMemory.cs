using System.Collections.Concurrent;

namespace ChanAdapter.Memory;

public static class ChanMemory
{
    private static readonly ConcurrentDictionary<string, object> Dict = new();
    private static readonly HashSet<float> Basekeys = [];

    private static string BuildCacheKey(float key, string suffix)
    {
        return $"{key}_{suffix}";
    }

    public static void Add(float baseKey, string suffix, object value)
    {
        //缓存中最多保存100 条图形窗口详情
        if (Basekeys.Add(baseKey) && (Basekeys.Count > 100 || Dict.Count > 1000))
        {
            var first = Basekeys.First();
            Basekeys.Remove(first);
        }

        Dict.AddOrUpdate(BuildCacheKey(baseKey, suffix), value, (k, v) => value);
    }

    public static T? Get<T>(float baseKey, string suffix) where T:class
    {
        return Dict.TryGetValue(BuildCacheKey(baseKey, suffix), out var value)
            ? value as T
            : null;
    }


    public static void Remove(float baseKey, string suffix)
    {
        Dict.TryRemove(BuildCacheKey(baseKey, suffix), out _);
    }

    public static void Clear()
    {
        Dict.Clear();
    }
}