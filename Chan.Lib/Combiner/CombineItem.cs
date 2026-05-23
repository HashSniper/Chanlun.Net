using Chan.Lib.Common;

namespace Chan.Lib.Combiner;

public interface ICombineSource
{
    object TimeBegin { get; }
    object TimeEnd { get; }
    float CombineHigh { get; }
    float CombineLow { get; }
}

public class CombineItem
{
    public object TimeBegin { get; }
    public object TimeEnd { get; }
    public float High { get; }
    public float Low { get; }

    public CombineItem(ICombineSource item)
    {
        TimeBegin = item.TimeBegin;
        TimeEnd = item.TimeEnd;
        High = item.CombineHigh;
        Low = item.CombineLow;
    }
}
