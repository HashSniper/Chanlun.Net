using Chan.Lib.Common;
using Chan.Lib.Bis;

namespace Chan.Lib.Seg;

public class ChanSegmentList : SegmentListBase
{
    public ChanSegmentList(SegmentConfig segConfig = null!, SEG_TYPE lv = SEG_TYPE.BI) : base(segConfig ?? new SegmentConfig(), lv)
    {
    }

    public override void Update(IReadOnlyList<IChanLine> biLst)
    {
        DoInit();
        if (Count == 0)
            CalSegSure(biLst, beginIdx: 0);
        else
            CalSegSure(biLst, beginIdx: this[^1].EndChan.Idx + 1);
        CollectLeftSeg(biLst);
    }

    private new void DoInit()
    {
        while (Count > 0 && !Lst[^1].IsSure)
        {
            var seg = Lst[^1];
            foreach (var bi in seg.BiList)
                bi.ParentSeg = null;
            if (seg.Pre != null)
                seg.Pre.Next = null;
            Lst.RemoveAt(Lst.Count - 1);
        }
        if (Count > 0)
        {
            if (Lst[^1].EigenFx == null || Lst[^1].EigenFx!.Ele[^1] == null)
                throw new InvalidOperationException();
            if (!Lst[^1].EigenFx!.Ele[^1]!.Lst[^1].IsSure)
                Lst.RemoveAt(Lst.Count - 1);
        }
    }

    private void CalSegSure(IReadOnlyList<IChanLine> biLst, int beginIdx)
    {
        var upEigen = new EigenFeature(CHAN_DIR.UP, lv: Lv);
        var downEigen = new EigenFeature(CHAN_DIR.DOWN, lv: Lv);
        CHAN_DIR? lastSegDir = Count == 0 ? null : this[^1].Dir;
        for (int i = beginIdx; i < biLst.Count; i++)
        {
            var bi = biLst[i];
            EigenFeature? fxEigen = null;
            if (bi.IsDown() && lastSegDir != CHAN_DIR.UP)
            {
                if (upEigen.Add(bi))
                    fxEigen = upEigen;
            }
            else if (bi.IsUp() && lastSegDir != CHAN_DIR.DOWN)
            {
                if (downEigen.Add(bi))
                    fxEigen = downEigen;
            }

            if (Count == 0)
            {
                if (upEigen.Ele[1] != null && bi.IsDown())
                {
                    lastSegDir = CHAN_DIR.DOWN;
                    downEigen.Clear();
                }
                else if (downEigen.Ele[1] != null && bi.IsUp())
                {
                    upEigen.Clear();
                    lastSegDir = CHAN_DIR.UP;
                }
                if (upEigen.Ele[1] == null && lastSegDir == CHAN_DIR.DOWN && bi.Dir == CHAN_DIR.DOWN)
                    lastSegDir = null;
                else if (downEigen.Ele[1] == null && lastSegDir == CHAN_DIR.UP && bi.Dir == CHAN_DIR.UP)
                    lastSegDir = null;
            }

            if (fxEigen != null)
            {
                TreatFxEigen(fxEigen, biLst);
                break;
            }
        }
    }

    private void TreatFxEigen(EigenFeature fxEigen, IReadOnlyList<IChanLine> biLst)
    {
        var test = fxEigen.CanBeEnd(biLst);
        int endBiIdx = fxEigen.GetPeakBiIdx();
        if (test == true || test == null)
        {
            bool isTrue = test != null;
            if (!AddNewSeg(biLst, endBiIdx, isSure: isTrue && fxEigen.AllBiIsSure()))
            {
                CalSegSure(biLst, endBiIdx + 1);
                return;
            }
            Lst[^1].EigenFx = fxEigen;
            if (isTrue)
                CalSegSure(biLst, endBiIdx + 1);
        }
        else
        {
            CalSegSure(biLst, fxEigen.Lst[1].Idx);
        }
    }
}
