using Chan.Lib.Common;

namespace Chan.Lib.Combiner;

public class Combiner<T> where T : class, ICombineSource
{
    public DateTime TimeBegin { get; private set; }
    public DateTime TimeEnd { get;private set; }
    public float High { get; private set; }
    public float Low { get; private set; }
    public List<T> Lst { get; }
    public Combiner_DIR Dir { get; }
    public FX_TYPE Fx { get; private set; }
    public Combiner<T>? Pre { get; private set; }
    public Combiner<T>? Next { get; private set; }


    protected Combiner(T data, Combiner_DIR dir)
    {
        var item = new CombineItem(data);
        TimeBegin = item.TimeBegin;
        TimeEnd = item.TimeEnd;
        High = item.High;
        Low = item.Low;
        Lst = [data];
        Dir = dir;
    }

    public Combiner_DIR TestCombine(CombineItem item, bool excludeIncluded = false, int? allowTopEqual = null)
    {
        if (High >= item.High && Low <= item.Low)
            return Combiner_DIR.COMBINE;
        if (High <= item.High && Low >= item.Low)
        {
            if (allowTopEqual == 1 && High == item.High && Low > item.Low)
                return Combiner_DIR.DOWN;
            if (allowTopEqual == -1 && Low == item.Low && High < item.High)
                return Combiner_DIR.UP;
            return excludeIncluded ? Combiner_DIR.INCLUDED : Combiner_DIR.COMBINE;
        }
        if (High > item.High && Low > item.Low)
            return Combiner_DIR.DOWN;
        if (High < item.High && Low < item.Low)
            return Combiner_DIR.UP;
        throw new ChanException("combine type unknown", ErrCode.COMBINER_ERR);
    }

    public void SetFx(FX_TYPE fx) => Fx = fx;

    public Combiner_DIR TryAdd(T item, bool excludeIncluded = false, int? allowTopEqual = null, bool skipUpdateInput = false)
    {
        var combineItem = new CombineItem(item);
        var dir = TestCombine(combineItem, excludeIncluded, allowTopEqual);
        if (dir == Combiner_DIR.COMBINE)
        {
            Lst.Add(item);
            if (Dir == Combiner_DIR.UP)
            {
                if (combineItem.High != combineItem.Low || combineItem.High != High)
                {
                    High = Math.Max(High, combineItem.High);
                    Low = Math.Max(Low, combineItem.Low);
                }
            }
            else if (Dir == Combiner_DIR.DOWN)
            {
                if (combineItem.High != combineItem.Low || combineItem.Low != Low)
                {
                    High = Math.Min(High, combineItem.High);
                    Low = Math.Min(Low, combineItem.Low);
                }
            }
            else
            {
                throw new ChanException($"Combiner_DIR = {Dir} err!!! must be UP/DOWN", ErrCode.COMBINER_ERR);
            }
            TimeEnd = combineItem.TimeEnd;
        }
        return dir;
    }

    public T GetPeakKlu(bool isHigh) => isHigh ? GetHighPeakKlu() : GetLowPeakKlu();

    public T GetHighPeakKlu()
    {
        foreach (var kl in Lst.AsEnumerable().Reverse())
        {
            if (kl.CombineHigh == High)
            {
                return kl;
            }
        }
        throw new ChanException("can't find peak...", ErrCode.COMBINER_ERR);
    }

    public T GetLowPeakKlu()
    {
        foreach (var kl in Lst.AsEnumerable().Reverse())
        {
            if (kl.CombineLow == Low)
            {
                return kl;
            }
        }
        throw new ChanException("can't find peak...", ErrCode.COMBINER_ERR);
    }

    public void UpdateFx(Combiner<T> pre, Combiner<T> next, bool excludeIncluded = false, int? allowTopEqual = null)
    {
        SetNext(next);
        SetPre(pre);
        next.SetPre(this);
        pre.SetNext(this);
        if (excludeIncluded)
        {
            if (pre.High < High && next.High <= High && next.Low < Low)
            {
                if (allowTopEqual == 1 || next.High < High)
                    Fx = FX_TYPE.TOP;
            }
            else if (next.High > High && pre.Low > Low && next.Low >= Low)
            {
                if (allowTopEqual == -1 || next.Low > Low)
                    Fx = FX_TYPE.BOTTOM;
            }
        }
        else if (pre.High < High && next.High < High && pre.Low < Low && next.Low < Low)
        {
            Fx = FX_TYPE.TOP;
        }
        else if (pre.High > High && next.High > High && pre.Low > Low && next.Low > Low)
        {
            Fx = FX_TYPE.BOTTOM;
        }
    }

    public override string ToString() => $"{TimeBegin}~{TimeEnd} {Low}->{High}";

    public T this[int index] => Lst[index];
    public int Count => Lst.Count;
    
    public void SetPre(Combiner<T>? pre)
    {
        Pre = pre;
    }

    public void SetNext(Combiner<T>? next)
    {
        Next = next;
    }
}
