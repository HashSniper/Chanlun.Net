using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.SEG;

namespace Chanlun.Lib.Extensions;

public static class EigenFxExtensions
{
    
    public static bool AddBi(this EigenFx self, Bi.Bi bi)
    {
        self.BiList.Add(bi);
        if (self.Eigens[0] == null)
        {
            return self.TreatFirstEigen(bi);
        }

        if (self.Eigens[1] == null)
        {
            return self.TreatSecondEigen(bi);
        }

        if (self.Eigens[2] == null)
        {
            return self.TreatThirdEigen(bi);
        }
        
        return false;
    }

    public static void TreatFxEigen(this EigenFx self, List<Bi.Bi> biList)
    {
        var peakBi = self.Eigens[1].PeakBi;
        var test = self.CanBeEnd(biList);
        var endBiIdx = peakBi.Idx - 1;
        if (test is true or null)
        {
            var isTure = test is not null;
            
        }
    }
    
    private static bool AddNewSeg

    private static bool? CanBeEnd(this EigenFx self, List<Bi.Bi> biList)
    {
        if (self.Eigens[1] != null && self.Eigens[1].Gap)
        {
            var peakBi = self.Eigens[1].PeakBi;
            var endBiIdx = peakBi.Idx - 1;
            var thredValue = biList[endBiIdx].GetEndValue();
            var breakThred = self.Dir == ChanDir.UP ? self.Eigens[0].Low : self.Eigens[0].High;
            return self.FindRevertFx(biList, endBiIdx + 2, thredValue, breakThred);
        }

        return self.Eigens[1] != null;
    }

    private static bool? FindRevertFx(this EigenFx self, List<Bi.Bi> biList, int beginIdx, float thredValue,
        float breakThred)
    {
        var first_bi_dir = biList[beginIdx].DIR; // down则是要找顶分型

        var egien_fx = new EigenFx(first_bi_dir == ChanDir.UP ? ChanDir.DOWN : ChanDir.UP);

        for (int i = beginIdx; i < biList.Count; i += 2)
        {
            var bi = biList[i];
            if (egien_fx.AddBi(bi))
            {
                while (true)
                {
                    var test = egien_fx.CanBeEnd(biList);
                    if (test is true or null)
                    {
                        self.LastEvidenceBi = bi;
                        return test;
                    }
                    else if (!egien_fx.ReSet())
                        break;
                }
            }
        }

        return null;
    }

    private static bool TreatFirstEigen(this EigenFx self, Bi.Bi bi)
    {
        self.Eigens[0] = new Eigen(self.Dir, bi);
        return false;
    }

    private static bool TreatSecondEigen(this EigenFx self, Bi.Bi bi)
    {
        if (self.Eigens[0].GetNewDirWithBi(bi)==null) return false;
        self.Eigens[1] = new Eigen(self.Dir, bi);

        if ((self.Dir == ChanDir.UP && self.Eigens[1].High < self.Eigens[0].High) ||
            (self.Dir == ChanDir.DOWN && self.Eigens[1].Low > self.Eigens[0].Low))
        {
            return self.ReSet();
        }

        return false;
    }

    private static bool TreatThirdEigen(this EigenFx self, Bi.Bi bi)
    {
        if (self.Eigens[0] == null || self.Eigens[1] == null)
        {
            return false;
        }

        self.LastEvidenceBi = bi;
        
        var dir = self.Eigens[1].GetNewDirWithBi(bi);
        if (dir == null)
        {
            return false;
        }
        
        self.Eigens[2] = new Eigen(dir.Value, bi);

        if (!self.ActualBreak())
        {
            return self.ReSet();
        }

        self.Eigens[1].UpdateFx(self.Eigens[0], self.Eigens[2]);
        var fx = self.Eigens[1].FX;

        if ((self.Dir == ChanDir.UP && fx == ChanFX.TOP) || (self.Dir == ChanDir.DOWN && fx == ChanFX.BOTTOM))
        {
            return true;
        }

        return self.ReSet();
    }

    private static bool ActualBreak(this EigenFx self)
    {
        if ((self.Dir == ChanDir.UP && self.Eigens[2].Low < self.Eigens[1].BiList.Last().Low) ||
            (self.Dir == ChanDir.DOWN && self.Eigens[2].High > self.Eigens[1].BiList.Last().High))
        {
            return true;
        }

        var ele2Bi = self.Eigens[2].BiList.First();
        if (ele2Bi.Next != null && ele2Bi.Next.Next != null)
        {
            if (ele2Bi.DIR == ChanDir.DOWN && ele2Bi.Next.Next.Low < ele2Bi.Low)
            {
                self.LastEvidenceBi = ele2Bi.Next.Next;
                return true;
            }

            if (ele2Bi.DIR == ChanDir.UP && ele2Bi.Next.Next.High > ele2Bi.High)
            {
                self.LastEvidenceBi = ele2Bi.Next.Next;
                return true;
            }
        }

        return false;
    }

    private static bool ReSet(this EigenFx self)
    {
        var biLListTemp = self.BiList.Skip(1).ToList();
        self.Clear();

        return biLListTemp.Any(self.AddBi);
    }
    
    
}