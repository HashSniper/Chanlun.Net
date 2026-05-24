using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.Bis;
using Chan.Lib.Seg;
using Chan.Lib.ZS;
using Chan.Lib.BuySellPoints;

namespace Chan.Lib.KLines;

public class KLineList(KL_TYPE klType, ChanConfig conf)
{
    public KL_TYPE KlType { get; } = klType;
    public ChanConfig Config { get; } = conf;
    public List<KLine> Lst { get; } = new();
    public BiList BiList { get; } = new(conf.BiConf);
    public SegmentListBase SegList { get; } = GetSeglistInstance(conf.SegConf, SEG_TYPE.BI);
    public SegmentListBase SegsegList { get; } = GetSeglistInstance(conf.SegConf, SEG_TYPE.SEG);
    public PivotList ZsList { get; } = new(conf.ZsConf);
    public PivotList SegzsList { get; } = new(conf.ZsConf);
    public BuySellPointList BsPointLst { get; } = new(conf.BsPointConf);
    public BuySellPointList SegBsPointLst { get; } = new(conf.SegBsPointConf);
    public bool StepCalculation { get; } = conf.TriggerStep;
    public int LastSureSegStartBiIdx { get; set; } = -1;
    public int LastSureSegsegStartBiIdx { get; set; } = -1;

    public KLine this[int index] => Lst[index];
    public List<KLine> this[System.Range range] => Lst[range];
    public int Count => Lst.Count;

    private void CalSegAndZs()
    {
        if (!StepCalculation)
            BiList.TryAddVirtualBi(Lst[^1]);
        LastSureSegStartBiIdx = CalSeg(BiList, SegList, LastSureSegStartBiIdx);
        ZsList.CalBiZs(BiList, SegList);
        UpdateZsInSeg(BiList, SegList, ZsList);

        LastSureSegsegStartBiIdx = CalSeg(SegList, SegsegList, LastSureSegsegStartBiIdx);
        SegzsList.CalBiZs(SegList, SegsegList);
        UpdateZsInSeg(SegList, SegsegList, SegzsList);

        SegBsPointLst.Cal(SegList, SegsegList);
        BsPointLst.Cal(BiList, SegList);
    }

    public void AddSingleKlu(KLineUnit klu)
    {
        if (Lst.Count == 0)
        {
            Lst.Add(new KLine(klu, idx: 0));
        }
        else
        {
            var dir = Lst[^1].TryAdd(klu);
            if (dir != Combiner_DIR.COMBINE)
            {
                Lst.Add(new KLine(klu, idx: Lst.Count, dir: dir));
                if (Lst.Count >= 3)
                    Lst[^2].UpdateFx(Lst[^3], Lst[^1]);
                if (BiList.UpdateBi(Lst[^2], Lst[^1], StepCalculation) && StepCalculation)
                {
                    CalSegAndZs();
                }
            }
            else if (StepCalculation && BiList.TryAddVirtualBi(Lst[^1], needDelEnd: true))
            {
                CalSegAndZs();
            }
        }
    }
    
    private static SegmentListBase GetSeglistInstance(SegmentConfig segConfig, SEG_TYPE lv)
    {
        return segConfig.SegAlgo switch
        {
            "chan" => new ChanSegmentList(segConfig, lv),
            "1+1" => new DyhSegmentList(segConfig, lv),
            "break" => new DefaultSegmentList(segConfig, lv),
            _ => throw new ChanException($"unsupport seg algorithm:{segConfig.SegAlgo}", ErrCode.PARA_ERROR)
        };
    }

    private static int CalSeg(BiList biList, SegmentListBase segList, int lastSureSegStartBiIdx)
    {
        segList.Update(biList);
        if (segList.Count == 0)
        {
            foreach (var bi in biList)
                bi.SetSegIdx(0);
            return -1;
        }

        var curSeg = segList[^1];
        int biIdx = biList.Count - 1;
        while (biIdx >= 0)
        {
            var bi = biList[biIdx];
            if (bi.SegIdx.HasValue && bi.Idx < lastSureSegStartBiIdx)
                break;
            if (bi.Idx > curSeg.EndChan.Idx)
            {
                bi.SetSegIdx(curSeg.Idx + 1);
                biIdx--;
                continue;
            }
            if (bi.Idx < curSeg.StartChan.Idx)
            {
                if (curSeg.Pre == null) throw new InvalidOperationException();
                curSeg = curSeg.Pre;
            }
            bi.SetSegIdx(curSeg.Idx);
            biIdx--;
        }

        lastSureSegStartBiIdx = -1;
        var seg = segList[^1];
        while (seg != null)
        {
            if (seg.IsSure)
            {
                lastSureSegStartBiIdx = seg.StartChan.Idx;
                break;
            }
            seg = seg.Pre;
        }
        return lastSureSegStartBiIdx;
    }

    private static int CalSeg(SegmentListBase segList, SegmentListBase segsegList, int lastSureSegsegStartBiIdx)
    {
        // same logic but for seg level
        segsegList.Update(segList);
        if (segsegList.Count == 0)
        {
            foreach (var seg in segList)
                seg.SetSegIdx(0);
            return -1;
        }

        var curSegseg = segsegList[^1];
        int segIdx = segList.Count - 1;
        while (segIdx >= 0)
        {
            var seg = segList[segIdx];
            if (seg.SegIdx.HasValue && seg.Idx < lastSureSegsegStartBiIdx)
                break;
            if (seg.Idx > curSegseg.EndChan.Idx)
            {
                seg.SetSegIdx(curSegseg.Idx + 1);
                segIdx--;
                continue;
            }
            if (seg.Idx < curSegseg.StartChan.Idx)
            {
                if (curSegseg.Pre == null) throw new InvalidOperationException();
                curSegseg = curSegseg.Pre;
            }
            seg.SetSegIdx(curSegseg.Idx);
            segIdx--;
        }

        lastSureSegsegStartBiIdx = -1;
        var ss = segsegList[^1];
        while (ss != null)
        {
            if (ss.IsSure)
            {
                lastSureSegsegStartBiIdx = ss.StartChan.Idx;
                break;
            }
            ss = ss.Pre;
        }
        return lastSureSegsegStartBiIdx;
    }

    private static void UpdateZsInSeg(BiList biList, SegmentListBase segList, PivotList zsList)
    {
        int sureSegCnt = 0;
        int segIdx = segList.Count - 1;
        while (segIdx >= 0)
        {
            var seg = segList[segIdx];
            if (seg.EleInsideIsSure)
                break;
            if (seg.IsSure)
                sureSegCnt++;
            seg.ClearZsLst();
            int zsIdx = zsList.Count - 1;
            while (zsIdx >= 0)
            {
                var zs = zsList[zsIdx];
                if (zs.EndUnit.Idx < seg.StartChan.GetBeginKlu().Idx)
                    break;
                if (zs.IsInside(seg))
                    seg.AddZs(zs);
                if (zs.BeginLine.Idx > 0)
                    zs.SetLineIn(biList[zs.BeginLine.Idx - 1]);
                if (zs.EndLine.Idx + 1 < biList.Count)
                    zs.SetLineOut(biList[zs.EndLine.Idx + 1]);
                zs.SetLineLst(biList.Skip<IChanLine>(zs.BeginLine.Idx).Take(zs.EndLine.Idx - zs.BeginLine.Idx + 1).ToList());
                zsIdx--;
            }
            if (sureSegCnt > 2 && !seg.EleInsideIsSure)
                seg.EleInsideIsSure = true;
            segIdx--;
        }
    }

    private static void UpdateZsInSeg(SegmentListBase segList, SegmentListBase segsegList, PivotList zsList)
    {
        int sureSegCnt = 0;
        int segIdx = segsegList.Count - 1;
        while (segIdx >= 0)
        {
            var seg = segsegList[segIdx];
            if (seg.EleInsideIsSure)
                break;
            if (seg.IsSure)
                sureSegCnt++;
            seg.ClearZsLst();
            int zsIdx = zsList.Count - 1;
            while (zsIdx >= 0)
            {
                var zs = zsList[zsIdx];
                if (zs.EndUnit.Idx < seg.StartChan.GetBeginKlu().Idx)
                    break;
                if (zs.IsInside(seg))
                    seg.AddZs(zs);
                if (zs.BeginLine.Idx > 0)
                    zs.SetLineIn(segList[zs.BeginLine.Idx - 1]);
                if (zs.EndLine.Idx + 1 < segList.Count)
                    zs.SetLineOut(segList[zs.EndLine.Idx + 1]);
                zs.SetLineLst(segList.Skip<IChanLine>(zs.BeginLine.Idx).Take(zs.EndLine.Idx - zs.BeginLine.Idx + 1).ToList());
                zsIdx--;
            }
            if (sureSegCnt > 2 && !seg.EleInsideIsSure)
                seg.EleInsideIsSure = true;
            segIdx--;
        }
    }
}
