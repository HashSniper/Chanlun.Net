namespace Chanlun.Lib.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// 安全判断集合是否为 null 或空
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }
    
    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return !source.IsNullOrEmpty();
    }

    public static void RemoveEnd<T>(this IList<T>? source)
    {
        if (source.IsNullOrEmpty())
        {
            return;
        }

        source.RemoveAt(source.Count - 1);
    }
}