using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;

namespace Chanlun.Lib.SEG;

public static class SegCalculator
{
    public static void Calculate(float key, List<Bi.Bi> bis, ref ChanCalculateResult result)
    {
        var segs = new List<Seg>();
        if (bis.IsNullOrEmpty())
        {
            return;
        }

        CreateEigenFx(0, bis, segs);
        CollectLeftSeg(bis, segs);

        result.SegList = segs;
    }

    public static void Update(List<Bi.Bi> bis, List<Seg> segs)
    {
        DoInit(segs);
        CreateEigenFx(segs.Count > 0 ? segs.Last().EndBi.Idx + 1 : 0, bis, segs);
        CollectLeftSeg(bis, segs);
    }

    private static void CollectLeftSeg(List<Bi.Bi> biLst, List<Seg> segs)
    {
        if (segs.Count == 0)
            CollectFirstSeg(biLst, segs);
        else
            CollectSegs(biLst, segs);
    }

    private static void CollectSegs(List<Bi.Bi> biLst, List<Seg> segs)
    {
        var lastBi = biLst[biLst.Count - 1];
        var lastSegEndBi = segs[^1].EndBi;
        if (lastBi.Idx - lastSegEndBi.Idx < 3)
            return;
        if (lastSegEndBi.DIR.IsDown() && lastBi.GetEndValue() <= lastSegEndBi.GetEndValue())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3).ToList(), isHigh: true);
            if (peakBi != null)
            {
                AddNewSeg(biLst, segs, peakBi.Idx, isSure: false, segDir: ChanDir.UP);
                CollectLeftSeg(biLst, segs);
            }
        }
        else if (lastSegEndBi.DIR.IsUp() && lastBi.GetEndValue() >= lastSegEndBi.GetEndValue())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3).ToList(), isHigh: false);
            if (peakBi != null)
            {
                AddNewSeg(biLst, segs, peakBi.Idx, isSure: false, segDir: ChanDir.DOWN);
                CollectLeftSeg(biLst, segs);
            }
        }

        CollectLeftSegPeakMethod(lastSegEndBi, biLst, segs);
    }

    private static void CollectLeftSegPeakMethod(Bi.Bi lastSegEndBi, List<Bi.Bi> biLst, List<Seg> segs)
    {
        bool findNewSeg = false;
        if (lastSegEndBi.DIR.IsDown())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3).ToList(), isHigh: true);
            if (peakBi != null && peakBi.Idx - lastSegEndBi.Idx >= 3)
            {
                AddNewSeg(biLst, segs, peakBi.Idx, isSure: false, segDir: ChanDir.UP);
                findNewSeg = true;
            }
        }
        else
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3).ToList(), isHigh: false);
            if (peakBi != null && peakBi.Idx - lastSegEndBi.Idx >= 3)
            {
                AddNewSeg(biLst, segs, peakBi.Idx, isSure: false, segDir: ChanDir.DOWN);
                findNewSeg = true;
            }
        }

        lastSegEndBi = segs[^1].EndBi;
        if (!findNewSeg)
            CollectLeftAsSeg(biLst, segs);
        else
            CollectLeftSegPeakMethod(lastSegEndBi, biLst, segs);
    }

    private static void CollectFirstSeg(List<Bi.Bi> biLst, List<Seg> segs)
    {
        if (biLst.Count < 3) return;
        float high = biLst.Max(b => b.High);
        float low = biLst.Min(b => b.Low);
        if (Math.Abs(high - biLst[0].GetBeginValue()) >= Math.Abs(low - biLst[0].GetBeginValue()))
        {
            var peakBi = FindPeakBi(biLst, isHigh: true);
            if (peakBi == null) throw new InvalidOperationException();
            AddNewSeg(biLst, segs, peakBi.Idx, isSure: false, segDir: ChanDir.UP, splitFirstSeg: false);
        }
        else
        {
            var peakBi = FindPeakBi(biLst, isHigh: false);
            if (peakBi == null) throw new InvalidOperationException();
            AddNewSeg(biLst, segs, peakBi.Idx, isSure: false, segDir: ChanDir.DOWN, splitFirstSeg: false);
        }

        CollectLeftAsSeg(biLst, segs);
    }

    private static void CollectLeftAsSeg(List<Bi.Bi> biLst, List<Seg> segs)
    {
        var lastBi = biLst.Last();
        var lastSegEndBi = segs[^1].EndBi;
        if (lastSegEndBi.Idx + 1 >= biLst.Count)
            return;
        if (lastSegEndBi.DIR == lastBi.DIR)
            AddNewSeg(biLst, segs, lastBi.Idx - 1, isSure: false);
        else
            AddNewSeg(biLst, segs, lastBi.Idx, isSure: false);
    }


    private static void DoInit(List<Seg> segs)
    {
        while (segs.Count > 0 && !segs[^1].IsSure)
        {
            var seg = segs[^1];
            if (seg.Pre != null)
                seg.Pre.Next = null;
            segs.RemoveAt(segs.Count - 1);
        }

        if (segs.Count > 0)
        {
            if (!(segs[^1].EigenFx!.Eigen1[^1]!.IsSure == true))
                segs.RemoveEnd();
        }
    }


    public static float[] GetSegs(int nCount, float[] pHigh, float[] pLow, float[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var pOut = new float[nCount];
        if (calculateResult == null)
        {
            return pOut;
        }

        var segList = calculateResult.SegList;
        foreach (var seg in segList)
        {
            if (seg.DIR.IsUp())
            {
                pOut[seg.EndBi.EndKLine.PeakUnit.Idx] = 1;
                pOut[seg.StartBi.StartKLine.PeakUnit.Idx] = -1;
            }
            else if (seg.DIR.IsDown())
            {
                pOut[seg.EndBi.EndKLine.PeakUnit.Idx] = -1;
                pOut[seg.StartBi.StartKLine.PeakUnit.Idx] = 1;
            }
        }

        return pOut;
    }

    private static void CreateEigenFx(int startBiIdx, List<Bi.Bi> biLst, List<Seg> segs)
    {
        var upEigen = new EigenFx(ChanDir.UP);
        var downEigen = new EigenFx(ChanDir.DOWN);
        ChanDir? lastSegDir = segs.Count == 0 ? null : segs[^1].DIR;
        for (int i = startBiIdx; i < biLst.Count; i++)
        {
            var bi = biLst[i];
            EigenFx? fxEigen = null;
            if (bi.DIR.IsDown() && !lastSegDir.IsUp())
            {
                if (upEigen.Add(bi))
                    fxEigen = upEigen;
            }
            else if (bi.DIR.IsUp() && !lastSegDir.IsDown())
            {
                if (downEigen.Add(bi))
                    fxEigen = downEigen;
            }

            if (segs.Count == 0)
            {
                if (upEigen.Eigen1 != null && bi.DIR.IsDown())
                {
                    lastSegDir = ChanDir.DOWN;
                    downEigen.Clear();
                }
                else if (downEigen.Eigen1 != null && bi.DIR.IsUp())
                {
                    upEigen.Clear();
                    lastSegDir = ChanDir.UP;
                }

                if ((upEigen.Eigen1 == null && lastSegDir.IsDown() && bi.DIR.IsDown()) ||
                    (downEigen.Eigen1 == null && lastSegDir.IsUp() && bi.DIR.IsUp()))
                {
                    lastSegDir = null;
                }
            }

            if (fxEigen != null)
            {
                TreatFxEigen(fxEigen, biLst, segs);
                break;
            }
        }
    }

    private static void TreatFxEigen(EigenFx fxEigen, List<Bi.Bi> biLst, List<Seg> segs)
    {
        var test = fxEigen.CanBeEnd(biLst);
        int endBiIdx = fxEigen.GetPeakBiIdx();
        if (test == true || test == null)
        {
            bool isTrue = test != null;
            AddNewSeg(biLst, segs, endBiIdx, isSure: isTrue && fxEigen.AllBiIsSure());
            segs[^1].EigenFx = fxEigen;
            CreateEigenFx(endBiIdx + 1, biLst, segs);
        }
        else
        {
            CreateEigenFx(fxEigen.Lst[1].Idx, biLst, segs);
        }
    }

    private static void AddNewSeg(List<Bi.Bi> biLst, List<Seg> segs, int endBiIdx, bool isSure = true,
        ChanDir? segDir = null, bool splitFirstSeg = true)
    {
        if (segs.Count == 0 && splitFirstSeg && endBiIdx >= 3)
        {
            var peakBi = FindPeakBi(biLst.Take(endBiIdx - 2).Reverse().ToList(), biLst[endBiIdx].DIR.IsDown());
            if (peakBi != null)
            {
                if ((peakBi.DIR.IsDown() && (peakBi.Low < biLst[0].Low || peakBi.Idx == 0)) ||
                    (peakBi.DIR.IsUp() && (peakBi.High > biLst[0].High || peakBi.Idx == 0)))
                {
                    AddNewSeg(biLst, segs, peakBi.Idx, isSure: false, segDir: peakBi.DIR);
                    AddNewSeg(biLst, segs, endBiIdx, isSure: false);
                    return;
                }
            }
        }
        
        int bi1Idx = segs.Count == 0 ? 0 : segs[^1].EndBi.Idx + 1;
        if (bi1Idx >= biLst.Count)
        {
            return;
        }

        var bi1 = biLst[bi1Idx];
        var bi2 = biLst[endBiIdx];
        segs.Add(new Seg(segs.Count, bi1, bi2, isSure: isSure));

        if (segs.Count >= 2)
        {
            segs[^2].Next = segs[^1];
            segs[^1].Pre = segs[^2];
        }

        segs[^1].UpdateBiList(biLst.Skip(bi1Idx).Take(endBiIdx - bi1Idx + 1).Cast<Bi.Bi>().ToList(), 0,
            endBiIdx - bi1Idx);
    }

    private static Bi.Bi? FindPeakBi(IList<Bi.Bi> biLst, bool isHigh)
    {
        Bi.Bi? peakBi = null;
        float peakVal = isHigh ? float.NegativeInfinity : float.PositiveInfinity;

        foreach (var bi in biLst)
        {
            if ((isHigh && bi.GetEndValue() >= peakVal && bi.DIR.IsUp()) ||
                (!isHigh && bi.GetEndValue() <= peakVal && bi.DIR.IsDown()))
            {
                if (bi.Pre is { Pre: not null } &&
                    ((isHigh && bi.Pre.Pre.GetEndValue() > bi.GetEndValue()) ||
                     (!isHigh && bi.Pre.Pre.GetEndValue() < bi.GetEndValue())))
                {
                    continue;
                }

                peakVal = bi.GetEndValue();
                peakBi = bi;
            }
        }

        return peakBi;
    }
}