using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.KLines;

namespace Chan.Lib.Combiner;

public class KLineCombiner<T> where T : class, ICombineSource
{
    private object _timeBegin;
    private object _timeEnd;
    private float _high;
    private float _low;
    private readonly List<T> _lst;
    private KLINE_DIR _dir;
    private FX_TYPE _fx = FX_TYPE.UNKNOWN;
    private KLineCombiner<T>? _pre;
    private KLineCombiner<T>? _next;
    private readonly Dictionary<string, object> _memoizeCache = new();

    public object TimeBegin => _timeBegin;
    public object TimeEnd => _timeEnd;
    public float High => _high;
    public float Low => _low;
    public List<T> Lst => _lst;
    public KLINE_DIR Dir => _dir;
    public FX_TYPE Fx => _fx;
    public KLineCombiner<T>? Pre => _pre;
    public KLineCombiner<T>? Next => _next;

    public KLineCombiner<T> GetPre()
    {
        if (_pre == null) throw new InvalidOperationException("pre is null");
        return _pre;
    }

    public KLineCombiner<T>? GetNext() => _next;

    public void CleanCache() => _memoizeCache.Clear();

    public KLineCombiner(T klUnit, KLINE_DIR dir)
    {
        var item = new CombineItem(klUnit);
        _timeBegin = item.TimeBegin;
        _timeEnd = item.TimeEnd;
        _high = item.High;
        _low = item.Low;
        _lst = new List<T> { klUnit };
        _dir = dir;
    }

    public KLINE_DIR TestCombine(CombineItem item, bool excludeIncluded = false, int? allowTopEqual = null)
    {
        if (_high >= item.High && _low <= item.Low)
            return KLINE_DIR.COMBINE;
        if (_high <= item.High && _low >= item.Low)
        {
            if (allowTopEqual == 1 && _high == item.High && _low > item.Low)
                return KLINE_DIR.DOWN;
            if (allowTopEqual == -1 && _low == item.Low && _high < item.High)
                return KLINE_DIR.UP;
            return excludeIncluded ? KLINE_DIR.INCLUDED : KLINE_DIR.COMBINE;
        }
        if (_high > item.High && _low > item.Low)
            return KLINE_DIR.DOWN;
        if (_high < item.High && _low < item.Low)
            return KLINE_DIR.UP;
        throw new ChanException("combine type unknown", ErrCode.COMBINER_ERR);
    }

    public void SetFx(FX_TYPE fx) => _fx = fx;

    public KLINE_DIR TryAdd(T unitKl, bool excludeIncluded = false, int? allowTopEqual = null, bool skipUpdateInput = false)
    {
        var combineItem = new CombineItem(unitKl);
        var dir = TestCombine(combineItem, excludeIncluded, allowTopEqual);
        if (dir == KLINE_DIR.COMBINE)
        {
            _lst.Add(unitKl);
            if (unitKl is KLineUnit klu && !skipUpdateInput)
                klu.SetKlc((KLine)(object)this);
            if (_dir == KLINE_DIR.UP)
            {
                if (combineItem.High != combineItem.Low || combineItem.High != _high)
                {
                    _high = Math.Max(_high, combineItem.High);
                    _low = Math.Max(_low, combineItem.Low);
                }
            }
            else if (_dir == KLINE_DIR.DOWN)
            {
                if (combineItem.High != combineItem.Low || combineItem.Low != _low)
                {
                    _high = Math.Min(_high, combineItem.High);
                    _low = Math.Min(_low, combineItem.Low);
                }
            }
            else
            {
                throw new ChanException($"KLINE_DIR = {_dir} err!!! must be UP/DOWN", ErrCode.COMBINER_ERR);
            }
            _timeEnd = combineItem.TimeEnd;
            CleanCache();
        }
        return dir;
    }

    public T GetPeakKlu(bool isHigh) => isHigh ? GetHighPeakKlu() : GetLowPeakKlu();

    public T GetHighPeakKlu()
    {
        foreach (var kl in Lst.AsEnumerable().Reverse())
        {
            if (new CombineItem(kl).High == _high)
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
            if (new CombineItem(kl).Low == _low)
            {
                return kl;
            }
        }
        throw new ChanException("can't find peak...", ErrCode.COMBINER_ERR);
    }

    public void UpdateFx(KLineCombiner<T> pre, KLineCombiner<T> next, bool excludeIncluded = false, int? allowTopEqual = null)
    {
        SetNext(next);
        SetPre(pre);
        next.SetPre(this);
        if (excludeIncluded)
        {
            if (pre.High < _high && next.High <= _high && next.Low < _low)
            {
                if (allowTopEqual == 1 || next.High < _high)
                    _fx = FX_TYPE.TOP;
            }
            else if (next.High > _high && pre.Low > _low && next.Low >= _low)
            {
                if (allowTopEqual == -1 || next.Low > _low)
                    _fx = FX_TYPE.BOTTOM;
            }
        }
        else if (pre.High < _high && next.High < _high && pre.Low < _low && next.Low < _low)
        {
            _fx = FX_TYPE.TOP;
        }
        else if (pre.High > _high && next.High > _high && pre.Low > _low && next.Low > _low)
        {
            _fx = FX_TYPE.BOTTOM;
        }
        CleanCache();
    }

    public override string ToString() => $"{_timeBegin}~{_timeEnd} {_low}->{_high}";

    public T this[int index] => _lst[index];
    public List<T> this[System.Range range] => _lst[range];
    public int Count => _lst.Count;

    public System.Collections.Generic.IEnumerator<T> GetEnumerator() => _lst.GetEnumerator();

    public void SetPre(KLineCombiner<T>? pre)
    {
        _pre = pre;
        CleanCache();
    }

    public void SetNext(KLineCombiner<T>? next)
    {
        _next = next;
        CleanCache();
    }
}
