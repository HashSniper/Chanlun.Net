
namespace Chan.Lib.Combiner;

public interface ICombineSource
{
    DateTime TimeBegin { get; }
    DateTime TimeEnd { get; }
    float CombineHigh { get; }
    float CombineLow { get; }
}

public class CombineItem(ICombineSource item)
{
    public DateTime TimeBegin { get; } = item.TimeBegin;
    public DateTime TimeEnd { get; } = item.TimeEnd;
    public float High { get; } = item.CombineHigh;
    public float Low { get; } = item.CombineLow;
}
