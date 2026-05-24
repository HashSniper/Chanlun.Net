using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.KLines;
using Chan.Lib.Bis;
using Chan.Lib.Seg;
using Chan.Lib.BuySellPoints;

namespace Chan.Lib.ZS;

public class Pivot
{
    public bool IsSure { get; private set; }
    public List<Pivot> SubZsLst{ get; private set; }
    public KLineUnit BeginUnit{ get; private set; }
    public IChanLine BeginLine { get; private set; }
    public float Low { get; private set; }
    public float High { get; private set; }
    public float Mid { get; private set; }
    public KLineUnit EndUnit { get; private set; }
    public IChanLine EndLine { get; private set; }
    public float PeakHigh { get; private set; }
    public float PeakLow { get; private set; }
    public IChanLine? LineIn { get; private set; }
    public IChanLine? LineOut{ get; private set; }
    public List<IChanLine> LineLst { get; private set; }

    public Pivot(List<IChanLine>? lst, bool isSure = true)
    {
        IsSure = isSure;
        if (lst == null) return;

        BeginUnit = lst[0].GetBeginKlu();
        BeginLine = lst[0];
        UpdateZsRange(lst);

        foreach (var item in lst)
            UpdateZsEnd(item);
    }

    public void UpdateZsRange(List<IChanLine> lst)
    {
        Low = lst.Max(bi => bi.Low());
        High = lst.Min(bi => bi.High());
        Mid = (Low + High) / 2;
    }

    public bool IsOneBiZs()
    {
        if (EndLine == null) throw new InvalidOperationException();
        return BeginLine.Idx == EndLine.Idx;
    }

    public void UpdateZsEnd(IChanLine item)
    {
        EndUnit = item.GetEndKlu();
        EndLine = item;
        if (item.Low() < PeakLow)
            PeakLow = item.Low();
        if (item.High() > PeakHigh)
            PeakHigh = item.High();
    }

    public override string ToString()
    {
        var str = $"{BeginLine.Idx}->{EndLine.Idx}";
        var str2 = string.Join(",", SubZsLst.Select(sub => sub.ToString()));
        return string.IsNullOrEmpty(str2) ? str : $"{str}({str2})";
    }

    public bool Combine(Pivot zs2, string combineMode)
    {
        if (zs2.IsOneBiZs()) return false;
        if (BeginLine.SegIdx != zs2.BeginLine.SegIdx) return false;
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
        if (SubZsLst.Count == 0)
            SubZsLst.Add(MakeCopy());
        SubZsLst.Add(zs2);
        Low = Math.Min(Low, zs2.Low);
        High = Math.Max(High, zs2.High);
        PeakLow = Math.Min(PeakLow, zs2.PeakLow);
        PeakHigh = Math.Max(PeakHigh, zs2.PeakHigh);
        EndUnit = zs2.EndUnit;
        LineOut = zs2.LineOut;
        EndLine = zs2.EndLine;
    }

    public bool TryAddToEnd(IChanLine item)
    {
        if (!InRange(item)) return false;
        if (IsOneBiZs())
            UpdateZsRange(new List<IChanLine> { BeginLine, item });
        UpdateZsEnd(item);
        return true;
    }

    public bool InRange(IChanLine item) => FuncUtil.HasOverlap(Low, High, item.Low(), item.High());

    public bool IsInside(Segment seg) => seg.StartChan.Idx <= BeginLine.Idx && BeginLine.Idx <= seg.EndChan.Idx;

    public (bool isDivergence, double? rate) IsDivergence(PointConfig config, IChanLine? outBi = null)
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
        BeginUnit = zs.BeginUnit;
        EndUnit = zs.EndUnit;
        Low = zs.Low;
        High = zs.High;
        PeakHigh = zs.PeakHigh;
        PeakLow = zs.PeakLow;
        BeginLine = zs.BeginLine;
        EndLine = zs.EndLine;
        LineIn = zs.LineIn;
        LineOut = zs.LineOut;
    }

    public Pivot MakeCopy()
    {
        var copy = new Pivot(null, IsSure);
        copy.InitFromZs(zs: this);
        return copy;
    }

    public bool EndBiBreak(IChanLine? endBi = null)
    {
        endBi ??= GetBiOut();
        if (endBi == null) throw new InvalidOperationException();
        return (endBi.IsDown() && endBi.Low() < Low) || (endBi.IsUp() && endBi.High() > High);
    }

    public (bool isPeak, double? peakRate) OutBiIsPeak(int endBiIdx)
    {
        if (LineLst.Count == 0) throw new InvalidOperationException();
        if (LineOut == null) return (false, null);
        double peakRate = double.PositiveInfinity;
        foreach (var bi in LineLst)
        {
            if (bi.Idx > endBiIdx) break;
            if ((LineOut.IsDown() && bi.Low() < LineOut.Low()) || (LineOut.IsUp() && bi.High() > LineOut.High()))
                return (false, null);
            double r = Math.Abs(bi.GetEndVal() - LineOut.GetEndVal()) / LineOut.GetEndVal();
            if (r < peakRate) peakRate = r;
        }
        return (true, peakRate);
    }

    public IChanLine GetBiIn()
    {
        if (LineIn == null) throw new InvalidOperationException();
        return LineIn;
    }

    public IChanLine GetBiOut()
    {
        if (LineOut == null) throw new InvalidOperationException();
        return LineOut;
    }

    public void SetLineIn(IChanLine chan)
    {
        LineIn = chan;
    }

    public void SetLineOut(IChanLine chan)
    {
        LineOut = chan;
    }

    public void SetLineLst(List<IChanLine> list)
    {
        LineLst = list;
    }
}
