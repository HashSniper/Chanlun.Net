using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.Bis;

namespace Chan.Lib.Seg;

public class EigenFeature
{
    public SEG_TYPE Lv { get; }
    public BI_DIR Dir { get; }
    public Eigen?[] Ele { get; } = new Eigen?[3];
    public List<IBiLine> Lst { get; set; } = new();
    public bool ExcludeIncluded { get; }
    public KLINE_DIR KlDir { get; }
    public IBiLine? LastEvidenceBi { get; set; }

    public EigenFeature(BI_DIR dir, bool excludeIncluded = true, SEG_TYPE lv = SEG_TYPE.BI)
    {
        Lv = lv;
        Dir = dir;
        ExcludeIncluded = excludeIncluded;
        KlDir = dir == BI_DIR.UP ? KLINE_DIR.UP : KLINE_DIR.DOWN;
    }

    public bool Add(IBiLine bi)
    {
        if (bi.Dir == Dir) throw new ChanException($"特征序列方向错误! bi.dir={bi.Dir}, seg.dir={Dir}", ErrCode.SEG_EIGEN_ERR);
        Lst.Add(bi);
        if (Ele[0] == null)
            return TreatFirstEle(bi);
        else if (Ele[1] == null)
            return TreatSecondEle(bi);
        else if (Ele[2] == null)
            return TreatThirdEle(bi);
        else
            throw new ChanException($"特征序列3个都找齐了还没处理!! 当前笔:{bi.Idx},当前:{this}", ErrCode.SEG_EIGEN_ERR);
    }

    private bool TreatFirstEle(IBiLine bi)
    {
        Ele[0] = new Eigen(bi, KlDir);
        return false;
    }

    private bool TreatSecondEle(IBiLine bi)
    {
        if (Ele[0] == null) throw new InvalidOperationException();
        var combineDir = Ele[0].TryAdd(bi, excludeIncluded: ExcludeIncluded);
        if (combineDir != KLINE_DIR.COMBINE)
        {
            Ele[1] = new Eigen(bi, KlDir);
            if ((IsUp() && Ele[1].High < Ele[0].High) || (IsDown() && Ele[1].Low > Ele[0].Low))
                return Reset();
        }
        return false;
    }

    private bool TreatThirdEle(IBiLine bi)
    {
        if (Ele[0] == null || Ele[1] == null) throw new InvalidOperationException();
        LastEvidenceBi = bi;
        int? allowTopEqual = ExcludeIncluded ? (bi.IsDown() ? 1 : -1) : null;
        var combineDir = Ele[1].TryAdd(bi, allowTopEqual: allowTopEqual);
        if (combineDir == KLINE_DIR.COMBINE)
            return false;
        Ele[2] = new Eigen(bi, combineDir);
        if (!ActualBreak())
            return Reset();
        Ele[1]!.UpdateFx(Ele[0]!, Ele[2]!, excludeIncluded: ExcludeIncluded, allowTopEqual: allowTopEqual);
        var fx = Ele[1].Fx;
        bool isFx = (IsUp() && fx == FX_TYPE.TOP) || (IsDown() && fx == FX_TYPE.BOTTOM);
        return isFx ? true : Reset();
    }

    public bool Reset()
    {
        var biTmpList = Lst.Skip(1).ToList();
        if (ExcludeIncluded)
        {
            Clear();
            foreach (var bi in biTmpList)
                if (Add(bi)) return true;
        }
        else
        {
            if (Ele[1] == null) throw new InvalidOperationException();
            int ele2BeginIdx = Ele[1][0].Idx;
            Ele[0] = Ele[1];
            Ele[1] = Ele[2];
            Ele[2] = null;
            Lst = biTmpList.Where(b => b.Idx >= ele2BeginIdx).ToList();
        }
        return false;
    }

    public bool? CanBeEnd(IReadOnlyList<IBiLine> biLst)
    {
        if (Ele[1] == null) throw new InvalidOperationException();
        if (Ele[1].Gap)
        {
            if (Ele[0] == null) throw new InvalidOperationException();
            int endBiIdx = GetPeakBiIdx();
            double thredValue = biLst[endBiIdx].GetEndVal();
            double breakThred = IsUp() ? Ele[0].Low : Ele[0].High;
            return FindRevertFx(biLst, endBiIdx + 2, thredValue, breakThred);
        }
        return true;
    }

    public bool IsDown() => Dir == BI_DIR.DOWN;
    public bool IsUp() => Dir == BI_DIR.UP;

    public int GetPeakBiIdx()
    {
        if (Ele[1] == null) throw new InvalidOperationException();
        return Ele[1].GetPeakBiIdx();
    }

    public bool AllBiIsSure()
    {
        if (LastEvidenceBi == null) throw new InvalidOperationException();
        return Lst.All(bi => bi.IsSure) && LastEvidenceBi.IsSure;
    }

    public void Clear()
    {
        Ele[0] = null;
        Ele[1] = null;
        Ele[2] = null;
        Lst.Clear();
    }

    public override string ToString()
    {
        var t = Ele.Select(ele => ele == null ? "[]" : string.Join(",", ele.Lst.Select(b => b.Idx))).ToArray();
        return string.Join(" | ", t);
    }

    private bool ActualBreak()
    {
        if (!ExcludeIncluded) return true;
        if (Ele[2] == null || Ele[1] == null) throw new InvalidOperationException();
        if ((IsUp() && Ele[2].Low < Ele[1][^1].Low()) || (IsDown() && Ele[2].High > Ele[1][^1].High()))
            return true;
        if (Ele[2].Count != 1) throw new InvalidOperationException();
        var ele2Bi = Ele[2][0];
        if (ele2Bi.Next != null && ele2Bi.Next.Next != null)
        {
            if (ele2Bi.IsDown() && ele2Bi.Next.Next.Low() < ele2Bi.Low())
            {
                LastEvidenceBi = ele2Bi.Next.Next;
                return true;
            }
            else if (ele2Bi.IsUp() && ele2Bi.Next.Next.High() > ele2Bi.High())
            {
                LastEvidenceBi = ele2Bi.Next.Next;
                return true;
            }
        }
        return false;
    }

    private bool? FindRevertFx(IReadOnlyList<IBiLine> biList, int beginIdx, double thredValue, double breakThred)
    {
        bool commonCombine = false;
        var firstBiDir = biList[beginIdx].Dir;
        var eigenFx = new EigenFeature(FuncUtil.RevertBiDir(firstBiDir), excludeIncluded: !commonCombine, lv: Lv);
        for (int i = beginIdx; i < biList.Count; i += 2)
        {
            var bi = biList[i];
            if (eigenFx.Add(bi))
            {
                if (commonCombine) return true;
                while (true)
                {
                    var test = eigenFx.CanBeEnd(biList);
                    if (test == true || test == null)
                    {
                        LastEvidenceBi = bi;
                        return test;
                    }
                    else if (!eigenFx.Reset())
                        break;
                }
            }
        }
        return null;
    }
}
