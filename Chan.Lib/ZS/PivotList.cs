using Chan.Lib.Common;
using Chan.Lib.Bis;
using Chan.Lib.Seg;

namespace Chan.Lib.ZS;

public class PivotList
{
    public List<Pivot> ZsLst { get; } = new();
    public PivotConfig Config { get; }
    private List<IBiLine> _freeItemLst = new();
    public int LastSurePos { get; set; } = -1;
    public int LastSegIdx { get; set; } = 0;

    public PivotList(PivotConfig? zsConfig = null)
    {
        Config = zsConfig ?? new PivotConfig();
    }

    public void UpdateLastPos(SegmentListBase segList)
    {
        LastSurePos = -1;
        LastSegIdx = 0;
        for (int i = segList.Count - 1; i >= 0; i--)
        {
            var seg = segList[i];
            if (seg.IsSure)
            {
                LastSurePos = seg.StartBi.Idx;
                LastSegIdx = seg.Idx;
                return;
            }
        }
    }

    public bool SegNeedCal(Segment seg) => seg.StartBi.Idx >= LastSurePos;

    public void AddToFreeLst(IBiLine item, bool isSure, string zsAlgo)
    {
        if (_freeItemLst.Count > 0 && item.Idx == _freeItemLst[^1].Idx)
            _freeItemLst.RemoveAt(_freeItemLst.Count - 1);
        _freeItemLst.Add(item);
        var res = TryConstructZs(_freeItemLst, isSure, zsAlgo);
        if (res != null && res.BeginBi.Idx > 0)
        {
            ZsLst.Add(res);
            ClearFreeLst();
            TryCombine();
        }
    }

    public void ClearFreeLst() => _freeItemLst = new List<IBiLine>();

    public void Update(IBiLine bi, bool isSure = true)
    {
        if (_freeItemLst.Count == 0 && TryAddToEnd(bi))
        {
            TryCombine();
            return;
        }
        AddToFreeLst(bi, isSure, "normal");
    }

    public bool TryAddToEnd(IBiLine bi)
    {
        if (ZsLst.Count == 0) return false;
        return ZsLst[^1].TryAddToEnd(bi);
    }

    public void AddZsFromBiRange(List<IBiLine> segBiLst, BI_DIR segDir, bool segIsSure)
    {
        int dealBiCnt = 0;
        foreach (var bi in segBiLst)
        {
            if (bi.Dir == segDir) continue;
            if (dealBiCnt < 1)
            {
                AddToFreeLst(bi, segIsSure, "normal");
                dealBiCnt++;
            }
            else
            {
                Update(bi, segIsSure);
            }
        }
    }

    public Pivot? TryConstructZs(List<IBiLine> lst, bool isSure, string zsAlgo)
    {
        if (zsAlgo == "normal")
        {
            if (!Config.OneBiZs)
            {
                if (lst.Count == 1) return null;
                lst = lst.Skip(lst.Count - 2).ToList();
            }
        }
        else if (zsAlgo == "over_seg")
        {
            if (lst.Count < 3) return null;
            lst = lst.Skip(lst.Count - 3).ToList();
            if (lst[0].Dir == lst[0].ParentSeg?.Dir)
            {
                lst = lst.Skip(1).ToList();
                return null;
            }
        }
        double minHigh = lst.Min(item => item.High());
        double maxLow = lst.Max(item => item.Low());
        return minHigh > maxLow ? new Pivot(lst, isSure) : null;
    }

    public void CalBiZs(IReadOnlyList<IBiLine> biLst, SegmentListBase segLst)
    {
        while (ZsLst.Count > 0 && ZsLst[^1].BeginBi.Idx >= LastSurePos)
            ZsLst.RemoveAt(ZsLst.Count - 1);

        if (Config.ZsAlgo == "normal")
        {
            foreach (var seg in segLst.Skip<Segment>(LastSegIdx))
            {
                if (!SegNeedCal(seg)) continue;
                ClearFreeLst();
                var segBiLst = biLst.Skip(seg.StartBi.Idx).Take(seg.EndBi.Idx - seg.StartBi.Idx + 1).ToList();
                AddZsFromBiRange(segBiLst, seg.Dir, seg.IsSure);
            }
            if (segLst.Count > 0)
            {
                ClearFreeLst();
                var remaining = biLst.Skip(segLst[^1].EndBi.Idx + 1).ToList();
                AddZsFromBiRange(remaining, FuncUtil.RevertBiDir(segLst[^1].Dir), false);
            }
        }
        else if (Config.ZsAlgo == "over_seg")
        {
            if (!Config.OneBiZs) throw new InvalidOperationException();
            ClearFreeLst();
            int beginBiIdx = ZsLst.Count > 0 ? ZsLst[^1].EndBi.Idx + 1 : 0;
            for (int i = beginBiIdx; i < biLst.Count; i++)
                Update(biLst[i]);
        }
        else if (Config.ZsAlgo == "auto")
        {
            bool sureSegAppear = false;
            bool existSureSeg = segLst.ExistSureSeg();
            foreach (var seg in segLst.Skip<Segment>(LastSegIdx))
            {
                if (seg.IsSure) sureSegAppear = true;
                if (!SegNeedCal(seg)) continue;
                if (seg.IsSure || (!sureSegAppear && existSureSeg))
                {
                    ClearFreeLst();
                    var segBiLst = biLst.Skip(seg.StartBi.Idx).Take(seg.EndBi.Idx - seg.StartBi.Idx + 1).ToList();
                    AddZsFromBiRange(segBiLst, seg.Dir, seg.IsSure);
                }
                else
                {
                    ClearFreeLst();
                    for (int i = seg.StartBi.Idx; i < biLst.Count; i++)
                        Update(biLst[i]);
                    break;
                }
            }
        }
        else
        {
            throw new Exception($"unknown zs_algo {Config.ZsAlgo}");
        }
        UpdateLastPos(segLst);
    }

    public void UpdateOversegZs(IBiLine bi)
    {
        if (ZsLst.Count > 0 && _freeItemLst.Count == 0)
        {
            if (bi.Next == null) return;
            if (bi.Idx - ZsLst[^1].EndBi.Idx <= 1 && ZsLst[^1].InRange(bi.Next) && ZsLst[^1].TryAddToEnd(bi))
                return;
        }
        if (ZsLst.Count > 0 && _freeItemLst.Count == 0 && ZsLst[^1].InRange(bi) && bi.Idx - ZsLst[^1].EndBi.Idx <= 1)
            return;
        AddToFreeLst(bi, bi.IsSure, "over_seg");
    }

    public System.Collections.Generic.IEnumerator<Pivot> GetEnumerator() => ZsLst.GetEnumerator();
    public int Count => ZsLst.Count;
    public Pivot this[int index] => ZsLst[index];
    public List<Pivot> this[System.Range range] => ZsLst[range];

    public void TryCombine()
    {
        if (!Config.NeedCombine) return;
        while (ZsLst.Count >= 2 && ZsLst[^2].Combine(ZsLst[^1], Config.ZsCombineMode))
        {
            ZsLst.RemoveAt(ZsLst.Count - 1);
        }
    }
}
