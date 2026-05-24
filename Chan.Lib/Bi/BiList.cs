using Chan.Lib.Common;
using Chan.Lib.KLines;

namespace Chan.Lib.Bis;

public class BiList : IEnumerable<Bi>, IReadOnlyList<IChanLine>
{
    public List<Bi> BiList_ { get; } = new();
    public KLine? LastEnd { get; set; }
    public BiConfig Config { get; }
    private readonly List<KLine> _freeKlcLst = new();

    public BiList(BiConfig? biConf = null)
    {
        Config = biConf ?? new BiConfig();
    }

    public override string ToString() => string.Join("\n", BiList_.Select(b => b.ToString()));

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => BiList_.GetEnumerator();
    public System.Collections.Generic.IEnumerator<Bi> GetEnumerator() => BiList_.GetEnumerator();
    System.Collections.Generic.IEnumerator<IChanLine> System.Collections.Generic.IEnumerable<IChanLine>.GetEnumerator() => BiList_.GetEnumerator();
    public Bi this[int index] => BiList_[index];
    IChanLine IReadOnlyList<IChanLine>.this[int index] => BiList_[index];
    public List<Bi> this[System.Range range] => BiList_[range];
    public int Count => BiList_.Count;

    public bool TryCreateFirstBi(KLine klc)
    {
        foreach (var existFreeKlc in _freeKlcLst)
        {
            if (existFreeKlc.Fx == klc.Fx) continue;
            if (CanMakeBi(klc, existFreeKlc))
            {
                AddNewBi(existFreeKlc, klc);
                LastEnd = klc;
                return true;
            }
        }
        _freeKlcLst.Add(klc);
        LastEnd = klc;
        return false;
    }

    public bool UpdateBi(KLine klc, KLine lastKlc, bool calVirtual)
    {
        var flag1 = UpdateBiSure(klc);
        if (calVirtual)
        {
            var flag2 = TryAddVirtualBi(lastKlc);
            return flag1 || flag2;
        }
        return flag1;
    }

    public bool CanUpdatePeak(KLine klc)
    {
        if (Config.BiAllowSubPeak || BiList_.Count < 2) return false;
        if (BiList_[^1].IsDown() && klc.High < BiList_[^1].GetBeginVal()) return false;
        if (BiList_[^1].IsUp() && klc.Low > BiList_[^1].GetBeginVal()) return false;
        if (!BiListHelper.EndIsPeak(BiList_[BiList_.Count - 2].BeginKlc, klc)) return false;
        if (this[^1].IsDown() && this[^1].GetEndVal() < this[^2].GetBeginVal()) return false;
        if (this[^1].IsUp() && this[^1].GetEndVal() > this[^2].GetBeginVal()) return false;
        return true;
    }

    public bool UpdatePeak(KLine klc, bool forVirtual = false)
    {
        if (!CanUpdatePeak(klc)) return false;
        var tmpLastBi = BiList_[^1];
        BiList_.RemoveAt(BiList_.Count - 1);
        if (!TryUpdateEnd(klc, forVirtual: forVirtual))
        {
            BiList_.Add(tmpLastBi);
            return false;
        }
        else
        {
            if (forVirtual)
                BiList_[^1].AppendSureEnd(tmpLastBi.EndKlc);
            return true;
        }
    }

    public bool UpdateBiSure(KLine klc)
    {
        var tmpEnd = GetLastKluOfLastBi();
        DeleteVirtualBi();
        if (klc.Fx == FX_TYPE.UNKNOWN)
            return tmpEnd != GetLastKluOfLastBi();
        if (LastEnd == null || BiList_.Count == 0)
            return TryCreateFirstBi(klc);
        if (klc.Fx == LastEnd.Fx)
            return TryUpdateEnd(klc);
        else if (CanMakeBi(klc, LastEnd))
        {
            AddNewBi(LastEnd, klc);
            LastEnd = klc;
            return true;
        }
        else if (UpdatePeak(klc))
            return true;
        return tmpEnd != GetLastKluOfLastBi();
    }

    public void DeleteVirtualBi()
    {
        if (BiList_.Count > 0 && !BiList_[^1].IsSure)
        {
            var sureEndList = BiList_[^1].SureEnd.ToList();
            if (sureEndList.Count > 0)
            {
                BiList_[^1].RestoreFromVirtualEnd(sureEndList[0]);
                LastEnd = this[^1].EndKlc;
                foreach (var sureEnd in sureEndList.Skip(1))
                {
                    AddNewBi(LastEnd, sureEnd, isSure: true);
                    LastEnd = this[^1].EndKlc;
                }
            }
            else
            {
                BiList_.RemoveAt(BiList_.Count - 1);
            }
        }
        LastEnd = Count > 0 ? this[^1].EndKlc : null;
        if (Count > 0)
            this[^1].Next = null;
    }

    public bool TryAddVirtualBi(KLine klc, bool needDelEnd = false)
    {
        if (needDelEnd)
            DeleteVirtualBi();
        if (Count == 0) return false;
        if (klc.Idx == this[^1].EndKlc.Idx) return false;
        if ((this[^1].IsUp() && klc.High >= this[^1].EndKlc.High) || (this[^1].IsDown() && klc.Low <= this[^1].EndKlc.Low))
        {
            BiList_[^1].UpdateVirtualEnd(klc);
            return true;
        }
        var tmpKlc = klc;
        while (tmpKlc != null && tmpKlc.Idx > this[BiList_.Count - 1].EndKlc.Idx)
        {
            if (CanMakeBi(tmpKlc, this[^1].EndKlc, forVirtual: true))
            {
                AddNewBi(LastEnd!, tmpKlc, isSure: false);
                return true;
            }
            else if (UpdatePeak(tmpKlc, forVirtual: true))
                return true;
            tmpKlc = tmpKlc.Pre as KLine;
        }
        return false;
    }

    public void AddNewBi(KLine preKlc, KLine curKlc, bool isSure = true)
    {
        BiList_.Add(new Bi(preKlc, curKlc, idx: BiList_.Count, isSure: isSure));
        if (BiList_.Count >= 2)
        {
            BiList_[^2].Next = BiList_[^1];
            BiList_[^1].Pre = BiList_[^2];
        }
    }

    public bool SatisfyBiSpan(KLine klc, KLine lastEnd)
    {
        int biSpan = GetKlcSpan(klc, lastEnd);
        if (Config.IsStrict)
            return biSpan >= 4;
        int uintKlCnt = 0;
        var tmpKlc = lastEnd.Next as KLine;
        while (tmpKlc != null)
        {
            uintKlCnt += tmpKlc.Count;
            if (tmpKlc.Next == null)
                return false;
            if (((KLine)tmpKlc.Next).Idx < klc.Idx)
                tmpKlc = (KLine)tmpKlc.Next;
            else
                break;
        }
        return biSpan >= 3 && uintKlCnt >= 3;
    }

    public int GetKlcSpan(KLine klc, KLine lastEnd)
    {
        int span = klc.Idx - lastEnd.Idx;
        if (!Config.GapAsKl) return span;
        if (span >= 4) return span;
        var tmpKlc = lastEnd;
        while (tmpKlc != null && tmpKlc.Idx < klc.Idx)
        {
            if (tmpKlc.HasGapWithNext())
                span++;
            tmpKlc = tmpKlc.Next as KLine;
        }
        return span;
    }

    public bool CanMakeBi(KLine klc, KLine lastEnd, bool forVirtual = false)
    {
        bool satisfySpan = Config.BiAlgo == "fx" || SatisfyBiSpan(klc, lastEnd);
        if (!satisfySpan) return false;
        if (!lastEnd.CheckFxValid(klc, Config.BiFxCheck, forVirtual)) return false;
        if (Config.BiEndIsPeak && !BiListHelper.EndIsPeak(lastEnd, klc)) return false;
        return true;
    }

    public bool TryUpdateEnd(KLine klc, bool forVirtual = false)
    {
        if (BiList_.Count == 0) return false;
        var lastBi = BiList_[BiList_.Count - 1];
        bool checkTop = forVirtual ? klc.Dir == Combiner_DIR.UP : klc.Fx == FX_TYPE.TOP;
        bool checkBottom = forVirtual ? klc.Dir == Combiner_DIR.DOWN : klc.Fx == FX_TYPE.BOTTOM;
        if ((lastBi.IsUp() && checkTop && klc.High >= lastBi.GetEndVal()) ||
            (lastBi.IsDown() && checkBottom && klc.Low <= lastBi.GetEndVal()))
        {
            if (forVirtual)
                lastBi.UpdateVirtualEnd(klc);
            else
                lastBi.UpdateNewEnd(klc);
            LastEnd = klc;
            return true;
        }
        return false;
    }

    public int? GetLastKluOfLastBi()
    {
        return Count > 0 ? this[^1].GetEndKlu().Idx : null;
    }
}

public static class BiListHelper
{
    public static bool EndIsPeak(KLine lastEnd, KLine curEnd)
    {
        if (lastEnd.Fx == FX_TYPE.BOTTOM)
        {
            double cmpThred = curEnd.High;
            var klc = lastEnd.Next as KLine;
            while (klc != null)
            {
                if (klc.Idx >= curEnd.Idx) return true;
                if (klc.High > cmpThred) return false;
                klc = klc.Next as KLine;
            }
        }
        else if (lastEnd.Fx == FX_TYPE.TOP)
        {
            double cmpThred = curEnd.Low;
            var klc = lastEnd.Next as KLine;
            while (klc != null)
            {
                if (klc.Idx >= curEnd.Idx) return true;
                if (klc.Low < cmpThred) return false;
                klc = klc.Next as KLine;
            }
        }
        return true;
    }
}
