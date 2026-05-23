using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.Bis;

namespace Chan.Lib.Seg;

public abstract class SegmentListBase : IEnumerable<Segment>, IReadOnlyList<IBiLine>
{
    public List<Segment> Lst { get; protected set; } = new();
    public SEG_TYPE Lv { get; }
    public SegmentConfig Config { get; }

    public SegmentListBase(SegmentConfig segConfig, SEG_TYPE lv)
    {
        Lv = lv;
        Config = segConfig;
        DoInit();
    }

    public void DoInit()
    {
        Lst = new List<Segment>();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Lst.GetEnumerator();
    public System.Collections.Generic.IEnumerator<Segment> GetEnumerator() => Lst.GetEnumerator();
    System.Collections.Generic.IEnumerator<IBiLine> System.Collections.Generic.IEnumerable<IBiLine>.GetEnumerator() => Lst.GetEnumerator();
    public Segment this[int index] => Lst[index];
    IBiLine IReadOnlyList<IBiLine>.this[int index] => Lst[index];
    public List<Segment> this[System.Range range] => Lst[range];
    public int Count => Lst.Count;

    public bool LeftBiBreak(BiList biLst)
    {
        if (Count == 0) return false;
        var lastSegEndBi = Lst[^1].EndBi;
        foreach (var bi in biLst.Skip<IBiLine>(lastSegEndBi.Idx + 1))
        {
            if (lastSegEndBi.IsUp() && bi.High() > lastSegEndBi.High()) return true;
            if (lastSegEndBi.IsDown() && bi.Low() < lastSegEndBi.Low()) return true;
        }
        return false;
    }

    public void CollectFirstSeg(IReadOnlyList<IBiLine> biLst)
    {
        if (biLst.Count < 3) return;
        if (Config.LeftMethod == LEFT_SEG_METHOD.PEAK)
        {
            double high = biLst.Max(b => b.High());
            double low = biLst.Min(b => b.Low());
            if (Math.Abs(high - biLst[0].GetBeginVal()) >= Math.Abs(low - biLst[0].GetBeginVal()))
            {
                var peakBi = FindPeakBi(biLst, isHigh: true);
                if (peakBi == null) throw new InvalidOperationException();
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: BI_DIR.UP, splitFirstSeg: false, reason: "0seg_find_high");
            }
            else
            {
                var peakBi = FindPeakBi(biLst, isHigh: false);
                if (peakBi == null) throw new InvalidOperationException();
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: BI_DIR.DOWN, splitFirstSeg: false, reason: "0seg_find_low");
            }
            CollectLeftAsSeg(biLst);
        }
        else if (Config.LeftMethod == LEFT_SEG_METHOD.ALL)
        {
            var dir = biLst[biLst.Count - 1].GetEndVal() >= biLst[0].GetBeginVal() ? BI_DIR.UP : BI_DIR.DOWN;
            AddNewSeg(biLst, biLst[biLst.Count - 1].Idx, isSure: false, segDir: dir, splitFirstSeg: false, reason: "0seg_collect_all");
        }
        else
        {
            throw new ChanException($"unknown seg left_method = {Config.LeftMethod}", ErrCode.PARA_ERROR);
        }
    }

    public void CollectLeftSegPeakMethod(IBiLine lastSegEndBi, IReadOnlyList<IBiLine> biLst)
    {
        bool findNewSeg = false;
        if (lastSegEndBi.IsDown())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3), isHigh: true);
            if (peakBi != null && peakBi.Idx - lastSegEndBi.Idx >= 3)
            {
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: BI_DIR.UP, reason: "collectleft_find_high");
                findNewSeg = true;
            }
        }
        else
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3), isHigh: false);
            if (peakBi != null && peakBi.Idx - lastSegEndBi.Idx >= 3)
            {
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: BI_DIR.DOWN, reason: "collectleft_find_low");
                findNewSeg = true;
            }
        }
        lastSegEndBi = Lst[^1].EndBi;
        if (!findNewSeg)
            CollectLeftAsSeg(biLst);
        else
            CollectLeftSegPeakMethod(lastSegEndBi, biLst);
    }

    public void CollectSegs(IReadOnlyList<IBiLine> biLst)
    {
        var lastBi = biLst[biLst.Count - 1];
        var lastSegEndBi = Lst[^1].EndBi;
        if (lastBi.Idx - lastSegEndBi.Idx < 3)
            return;
        if (lastSegEndBi.IsDown() && lastBi.GetEndVal() <= lastSegEndBi.GetEndVal())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3), isHigh: true);
            if (peakBi != null)
            {
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: BI_DIR.UP, reason: "collectleft_find_high_force");
                CollectLeftSeg(biLst);
            }
        }
        else if (lastSegEndBi.IsUp() && lastBi.GetEndVal() >= lastSegEndBi.GetEndVal())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3), isHigh: false);
            if (peakBi != null)
            {
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: BI_DIR.DOWN, reason: "collectleft_find_low_force");
                CollectLeftSeg(biLst);
            }
        }
        else if (Config.LeftMethod == LEFT_SEG_METHOD.ALL)
        {
            CollectLeftAsSeg(biLst);
        }
        else if (Config.LeftMethod == LEFT_SEG_METHOD.PEAK)
        {
            CollectLeftSegPeakMethod(lastSegEndBi, biLst);
        }
        else
        {
            throw new ChanException($"unknown seg left_method = {Config.LeftMethod}", ErrCode.PARA_ERROR);
        }
    }

    public void CollectLeftSeg(IReadOnlyList<IBiLine> biLst)
    {
        if (Count == 0)
            CollectFirstSeg(biLst);
        else
            CollectSegs(biLst);
    }

    public void CollectLeftAsSeg(IReadOnlyList<IBiLine> biLst)
    {
        var lastBi = biLst[biLst.Count - 1];
        var lastSegEndBi = Lst[^1].EndBi;
        if (lastSegEndBi.Idx + 1 >= biLst.Count)
            return;
        if (lastSegEndBi.Dir == lastBi.Dir)
            AddNewSeg(biLst, lastBi.Idx - 1, isSure: false, reason: "collect_left_1");
        else
            AddNewSeg(biLst, lastBi.Idx, isSure: false, reason: "collect_left_0");
    }

    public void TryAddNewSeg(IReadOnlyList<IBiLine> biLst, int endBiIdx, bool isSure = true, BI_DIR? segDir = null, bool splitFirstSeg = true, string reason = "normal")
    {
        if (Count == 0 && splitFirstSeg && endBiIdx >= 3)
        {
            var peakBi = FindPeakBi(biLst.Take(endBiIdx - 2).Reverse(), biLst[endBiIdx].IsDown());
            if (peakBi != null)
            {
                if ((peakBi.IsDown() && (peakBi.Low() < biLst[0].Low() || peakBi.Idx == 0)) ||
                    (peakBi.IsUp() && (peakBi.High() > biLst[0].High() || peakBi.Idx == 0)))
                {
                    AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: peakBi.Dir, reason: "split_first_1st");
                    AddNewSeg(biLst, endBiIdx, isSure: false, reason: "split_first_2nd");
                    return;
                }
            }
        }
        int bi1Idx = Count == 0 ? 0 : Lst[^1].EndBi.Idx + 1;
        var bi1 = biLst[bi1Idx];
        var bi2 = biLst[endBiIdx];
        Lst.Add(new Segment(Lst.Count, (Bi)bi1, (Bi)bi2, isSure: isSure, segDir: segDir, reason: reason));

        if (Lst.Count >= 2)
        {
            Lst[^2].Next = Lst[^1];
            Lst[^1].Pre = Lst[^2];
        }
        Lst[^1].UpdateBiList(biLst.Skip(bi1Idx).Take(endBiIdx - bi1Idx + 1).Cast<Bi>().ToList(), 0, endBiIdx - bi1Idx);
    }

    public bool AddNewSeg(IReadOnlyList<IBiLine> biLst, int endBiIdx, bool isSure = true, BI_DIR? segDir = null, bool splitFirstSeg = true, string reason = "normal")
    {
        try
        {
            TryAddNewSeg(biLst, endBiIdx, isSure, segDir, splitFirstSeg, reason);
        }
        catch (ChanException e) when (e.ErrCode == ErrCode.SEG_END_VALUE_ERR && Lst.Count == 0)
        {
            return false;
        }
        catch (Exception)
        {
            throw;
        }
        return true;
    }

    public abstract void Update(IReadOnlyList<IBiLine> biLst);

    public bool ExistSureSeg() => Lst.Any(s => s.IsSure);

    public static IBiLine? FindPeakBi(IEnumerable<IBiLine> biLst, bool? isHigh = null)
    {
        IBiLine? peakBi = null;
        double peakVal = isHigh.HasValue && isHigh.Value ? double.NegativeInfinity : double.PositiveInfinity;
        foreach (var bi in biLst)
        {
            if (isHigh.HasValue)
            {
                if ((isHigh.Value && bi.GetEndVal() >= peakVal && bi.IsUp()) || (!isHigh.Value && bi.GetEndVal() <= peakVal && bi.IsDown()))
                {
                    if (bi.Pre != null && bi.Pre.Pre != null &&
                        ((isHigh.Value && bi.Pre.Pre.GetEndVal() > bi.GetEndVal()) || (!isHigh.Value && bi.Pre.Pre.GetEndVal() < bi.GetEndVal())))
                        continue;
                    peakVal = bi.GetEndVal();
                    peakBi = bi;
                }
            }
            else
            {
                bool highMode = isHigh ?? bi.IsDown();
                if ((highMode && bi.GetEndVal() >= peakVal && bi.IsUp()) || (!highMode && bi.GetEndVal() <= peakVal && bi.IsDown()))
                {
                    if (bi.Pre != null && bi.Pre.Pre != null &&
                        ((highMode && bi.Pre.Pre.GetEndVal() > bi.GetEndVal()) || (!highMode && bi.Pre.Pre.GetEndVal() < bi.GetEndVal())))
                        continue;
                    peakVal = bi.GetEndVal();
                    peakBi = bi;
                }
            }
        }
        return peakBi;
    }
}
