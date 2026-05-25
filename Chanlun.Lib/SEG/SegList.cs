using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;

namespace Chanlun.Lib.SEG;
using Bi= Bi.Bi;

public class SegList : List<Seg>
{
    public void CreateOrUpdateSeg(List<Bi> biList)
    {
        CreateEigenFx(0, biList);
        CollectLeftSeg(biList);
    }
    private void CollectLeftSeg(List<Bi> biLst)
    {
        if (this.Count == 0)
            CollectFirstSeg(biLst);
        else
            CollectSegs(biLst);
    }
    
    private void CollectFirstSeg(List<Bi> biLst)
    {
        if (biLst.Count < 3) return;
        float high = biLst.Max(b => b.High);
        float low = biLst.Min(b => b.Low);
        if (Math.Abs(high - biLst[0].GetBeginValue()) >= Math.Abs(low - biLst[0].GetBeginValue()))
        {
            var peakBi = FindPeakBi(biLst, isHigh: true);
            if (peakBi == null) throw new InvalidOperationException();
            AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: ChanDir.UP, splitFirstSeg: false);
        }
        else
        {
            var peakBi = FindPeakBi(biLst, isHigh: false);
            if (peakBi == null) throw new InvalidOperationException();
            AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: ChanDir.DOWN, splitFirstSeg: false);
        }

        CollectLeftAsSeg(biLst);
    }
    
    private void CollectSegs(List<Bi> biLst)
    {
        var lastBi = biLst.Last();
        var lastSegEndBi = this[^1].EndBi;
        if (lastBi.Idx - lastSegEndBi.Idx < 3)
            return;
        if (lastSegEndBi.DIR.IsDown() && lastBi.GetEndValue() <= lastSegEndBi.GetEndValue())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3).ToList(), isHigh: true);
            if (peakBi != null)
            {
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: ChanDir.UP);
                CollectLeftSeg(biLst);
            }
        }
        else if (lastSegEndBi.DIR.IsUp() && lastBi.GetEndValue() >= lastSegEndBi.GetEndValue())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3).ToList(), isHigh: false);
            if (peakBi != null)
            {
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: ChanDir.DOWN);
                CollectLeftSeg(biLst);
            }
        }

        CollectLeftSegPeakMethod(lastSegEndBi, biLst);
    }
    
    private void CollectLeftSegPeakMethod(Bi lastSegEndBi, List<Bi> biLst)
    {
        bool findNewSeg = false;
        if (lastSegEndBi.DIR.IsDown())
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3).ToList(), isHigh: true);
            if (peakBi != null && peakBi.Idx - lastSegEndBi.Idx >= 3)
            {
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: ChanDir.UP);
                findNewSeg = true;
            }
        }
        else
        {
            var peakBi = FindPeakBi(biLst.Skip(lastSegEndBi.Idx + 3).ToList(), isHigh: false);
            if (peakBi != null && peakBi.Idx - lastSegEndBi.Idx >= 3)
            {
                AddNewSeg(biLst, peakBi.Idx, isSure: false, segDir: ChanDir.DOWN);
                findNewSeg = true;
            }
        }

        lastSegEndBi = this[^1].EndBi;
        if (!findNewSeg)
            CollectLeftAsSeg(biLst);
        else
            CollectLeftSegPeakMethod(lastSegEndBi, biLst);
    }
    
    private void CollectLeftAsSeg(List<Bi> biLst)
    {
        var lastBi = biLst.Last();
        var lastSegEndBi = this[^1].EndBi;
        if (lastSegEndBi.Idx + 1 >= biLst.Count)
            return;
        if (lastSegEndBi.DIR == lastBi.DIR)
            AddNewSeg(biLst, lastBi.Idx - 1, isSure: false);
        else
            AddNewSeg(biLst, lastBi.Idx, isSure: false);
    }

    
    private void CreateEigenFx(int startBiIdx, List<Bi> biLst)
    {
        var upEigen = new EigenFx(ChanDir.UP);
        var downEigen = new EigenFx(ChanDir.DOWN);
        ChanDir? lastSegDir = Count == 0 ? null : this[^1].DIR;
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

            if (Count == 0)
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
                TreatFxEigen(fxEigen, biLst);
                break;
            }
        }
    }
    
    private void TreatFxEigen(EigenFx fxEigen, List<Bi> biLst)
    {
        var test = fxEigen.CanBeEnd(biLst);
        int endBiIdx = fxEigen.GetPeakBiIdx();
        if (test == true || test == null)
        {
            bool isTrue = test != null;
            AddNewSeg(biLst, endBiIdx, isSure: isTrue && fxEigen.AllBiIsSure());
            this[^1].EigenFx = fxEigen;
            CreateEigenFx(endBiIdx + 1, biLst);
        }
        else
        {
            CreateEigenFx(fxEigen.Lst[1].Idx, biLst);
        }
    }
    
    private void AddNewSeg(List<Bi> biLst, int endBiIdx, bool isSure = true,
        ChanDir? segDir = null, bool splitFirstSeg = true)
    {
        if (Count == 0 && splitFirstSeg && endBiIdx >= 3)
        {
            var peakBi = FindPeakBi(biLst.Take(endBiIdx - 2).Reverse().ToList(), biLst[endBiIdx].DIR.IsDown());
            if (peakBi != null)
            {
                if ((peakBi.DIR.IsDown() && (peakBi.Low < biLst[0].Low || peakBi.Idx == 0)) ||
                    (peakBi.DIR.IsUp() && (peakBi.High > biLst[0].High || peakBi.Idx == 0)))
                {
                    AddNewSeg(biLst,  peakBi.Idx, isSure: false, segDir: peakBi.DIR);
                    AddNewSeg(biLst,  endBiIdx, isSure: false);
                    return;
                }
            }
        }
        
        int bi1Idx = Count == 0 ? 0 : this[^1].EndBi.Idx + 1;
        if (bi1Idx >= biLst.Count)
        {
            return;
        }

        var bi1 = biLst[bi1Idx];
        var bi2 = biLst[endBiIdx];
        this.Add(new Seg(this.Count, bi1, bi2, isSure: isSure));

        if (this.Count >= 2)
        {
            this[^2].Next = this[^1];
            this[^1].Pre = this[^2];
        }

        this[^1].UpdateBiList(biLst.Skip(bi1Idx).Take(endBiIdx - bi1Idx + 1).Cast<Bi>().ToList(), 0,
            endBiIdx - bi1Idx);
    }
    
    private Bi? FindPeakBi(IList<Bi> biLst, bool isHigh)
    {
        Bi? peakBi = null;
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