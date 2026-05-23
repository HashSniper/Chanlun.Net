using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.KLines;
using Chan.Lib.Bis;
using Chan.Lib.Seg;
using Chan.Lib.BuySellPoints;

namespace Chan.Lib.ZS;

public class Pivot
{
    private bool _isSure;
    private List<Pivot> _subZsLst = new();
    private KLineUnit _begin;
    private IBiLine _beginBi;
    private float _low;
    private float _high;
    private float _mid;
    private KLineUnit _end;
    private IBiLine _endBi;
    private float _peakHigh = float.NegativeInfinity;
    private float _peakLow = float.PositiveInfinity;
    private IBiLine? _biIn;
    private IBiLine? _biOut;
    private List<IBiLine> _biLst = new();
    private readonly Dictionary<string, object> _memoizeCache = new();

    public bool IsSure => _isSure;
    public List<Pivot> SubZsLst => _subZsLst;
    public KLineUnit Begin => _begin;
    public IBiLine BeginBi => _beginBi;
    public float Low => _low;
    public float High => _high;
    public float Mid => _mid;
    public KLineUnit End => _end;
    public IBiLine EndBi => _endBi;
    public float PeakHigh => _peakHigh;
    public float PeakLow => _peakLow;
    public IBiLine? BiIn => _biIn;
    public IBiLine? BiOut => _biOut;
    public List<IBiLine> BiLst => _biLst;

    public void CleanCache() => _memoizeCache.Clear();

    public Pivot(List<IBiLine>? lst, bool isSure = true)
    {
        _isSure = isSure;
        if (lst == null) return;

        _begin = lst[0].GetBeginKlu();
        _beginBi = lst[0];
        UpdateZsRange(lst);

        foreach (var item in lst)
            UpdateZsEnd(item);
    }

    public void UpdateZsRange(List<IBiLine> lst)
    {
        _low = lst.Max(bi => bi.Low());
        _high = lst.Min(bi => bi.High());
        _mid = (_low + _high) / 2;
        CleanCache();
    }

    public bool IsOneBiZs()
    {
        if (_endBi == null) throw new InvalidOperationException();
        return _beginBi.Idx == _endBi.Idx;
    }

    public void UpdateZsEnd(IBiLine item)
    {
        _end = item.GetEndKlu();
        _endBi = item;
        if (item.Low() < _peakLow)
            _peakLow = item.Low();
        if (item.High() > _peakHigh)
            _peakHigh = item.High();
        CleanCache();
    }

    public override string ToString()
    {
        var str = $"{_beginBi.Idx}->{_endBi.Idx}";
        var str2 = string.Join(",", _subZsLst.Select(sub => sub.ToString()));
        return string.IsNullOrEmpty(str2) ? str : $"{str}({str2})";
    }

    public bool Combine(Pivot zs2, string combineMode)
    {
        if (zs2.IsOneBiZs()) return false;
        if (_beginBi.SegIdx != zs2._beginBi.SegIdx) return false;
        if (combineMode == "zs")
        {
            if (!FuncUtil.HasOverlap(Low, High, zs2.Low, zs2.High, equal: true))
                return false;
            DoCombine(zs2);
            return true;
        }
        else if (combineMode == "peak")
        {
            if (FuncUtil.HasOverlap(PeakLow, PeakHigh, zs2.PeakLow, zs2.PeakHigh))
            {
                DoCombine(zs2);
                return true;
            }
            return false;
        }
        else
        {
            throw new ChanException($"{combineMode} is unsupport zs conbine mode", ErrCode.PARA_ERROR);
        }
    }

    private void DoCombine(Pivot zs2)
    {
        if (_subZsLst.Count == 0)
            _subZsLst.Add(MakeCopy());
        _subZsLst.Add(zs2);
        _low = Math.Min(_low, zs2.Low);
        _high = Math.Max(_high, zs2.High);
        _peakLow = Math.Min(_peakLow, zs2.PeakLow);
        _peakHigh = Math.Max(_peakHigh, zs2.PeakHigh);
        _end = zs2.End;
        _biOut = zs2.BiOut;
        _endBi = zs2.EndBi;
        CleanCache();
    }

    public bool TryAddToEnd(IBiLine item)
    {
        if (!InRange(item)) return false;
        if (IsOneBiZs())
            UpdateZsRange(new List<IBiLine> { _beginBi, item });
        UpdateZsEnd(item);
        return true;
    }

    public bool InRange(IBiLine item) => FuncUtil.HasOverlap(Low, High, item.Low(), item.High());

    public bool IsInside(Segment seg) => seg.StartBi.Idx <= _beginBi.Idx && _beginBi.Idx <= seg.EndBi.Idx;

    public (bool isDivergence, double? rate) IsDivergence(PointConfig config, IBiLine? outBi = null)
    {
        if (!EndBiBreak(outBi)) return (false, null);
        double inMetric = GetBiIn().CalMacdMetric(config.MacdAlgo, isReverse: false);
        double outMetric = (outBi ?? GetBiOut()).CalMacdMetric(config.MacdAlgo, isReverse: true);
        if (config.DivergenceRate > 100)
            return (true, outMetric / inMetric);
        return (outMetric <= config.DivergenceRate * inMetric, outMetric / inMetric);
    }

    public void InitFromZs(Pivot zs)
    {
        _begin = zs.Begin;
        _end = zs.End;
        _low = zs.Low;
        _high = zs.High;
        _peakHigh = zs.PeakHigh;
        _peakLow = zs.PeakLow;
        _beginBi = zs.BeginBi;
        _endBi = zs.EndBi;
        _biIn = zs.BiIn;
        _biOut = zs.BiOut;
    }

    public Pivot MakeCopy()
    {
        var copy = new Pivot(null, _isSure);
        copy.InitFromZs(zs: this);
        return copy;
    }

    public bool EndBiBreak(IBiLine? endBi = null)
    {
        endBi ??= GetBiOut();
        if (endBi == null) throw new InvalidOperationException();
        return (endBi.IsDown() && endBi.Low() < _low) || (endBi.IsUp() && endBi.High() > _high);
    }

    public (bool isPeak, double? peakRate) OutBiIsPeak(int endBiIdx)
    {
        if (_biLst.Count == 0) throw new InvalidOperationException();
        if (_biOut == null) return (false, null);
        double peakRate = double.PositiveInfinity;
        foreach (var bi in _biLst)
        {
            if (bi.Idx > endBiIdx) break;
            if ((_biOut.IsDown() && bi.Low() < _biOut.Low()) || (_biOut.IsUp() && bi.High() > _biOut.High()))
                return (false, null);
            double r = Math.Abs(bi.GetEndVal() - _biOut.GetEndVal()) / _biOut.GetEndVal();
            if (r < peakRate) peakRate = r;
        }
        return (true, peakRate);
    }

    public IBiLine GetBiIn()
    {
        if (_biIn == null) throw new InvalidOperationException();
        return _biIn;
    }

    public IBiLine GetBiOut()
    {
        if (_biOut == null) throw new InvalidOperationException();
        return _biOut;
    }

    public void SetBiIn(IBiLine bi)
    {
        _biIn = bi;
        CleanCache();
    }

    public void SetBiOut(IBiLine bi)
    {
        _biOut = bi;
        CleanCache();
    }

    public void SetBiLst(List<IBiLine> biLst)
    {
        _biLst = biLst;
        CleanCache();
    }
}
