using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;

namespace Chanlun.Lib.SEG;

public class EigenFx(ChanDir dir)
{
    public Eigen? Eigen0 { get; set; }
    public Eigen? Eigen1 { get; set; }
    public Eigen? Eigen2 { get; set; }
    public List<Bi.Bi> Lst { get; set; } = [];

    public Bi.Bi? LastEvidenceBi { get; set; }

    /// <summary>
    /// 特征序列方向和线段的方向一致，组成特征序列的笔的方向和段的方向相反
    /// </summary>
    public ChanDir DIR { get; } = dir;

    public bool Add(Bi.Bi bi)
    {
        if (bi.DIR == DIR) throw new Exception($"特征序列方向错误! bi.dir={bi.DIR}, seg.dir={DIR}");
        Lst.Add(bi);
        if (Eigen0 == null)
            return TreatFirstEle(bi);
        else if (Eigen1 == null)
            return TreatSecondEle(bi);
        else if (Eigen2 == null)
            return TreatThirdEle(bi);
        else
            throw new Exception($"特征序列3个都找齐了还没处理!! 当前笔:{bi.Idx},当前:{this}");
    }

    private bool TreatFirstEle(Bi.Bi bi)
    {
        Eigen0 = new Eigen(0, DIR, bi);
        return false;
    }

    private bool TreatSecondEle(Bi.Bi bi)
    {
        if (Eigen0 == null) throw new InvalidOperationException();
        var combineDir = Eigen0.TryAdd(bi);
        if (combineDir != ChanDir.COMBINE)
        {
            Eigen1 = new Eigen(1, DIR, bi);
            if ((DIR.IsUp() && Eigen1.High < Eigen0.High) || (DIR.IsDown() && Eigen1.Low > Eigen0.Low))
                return Reset();
        }

        return false;
    }

    private bool TreatThirdEle(Bi.Bi bi)
    {
        if (Eigen0 == null || Eigen1 == null) throw new InvalidOperationException();
        LastEvidenceBi = bi;
        int allowTopEqual = bi.DIR.IsDown() ? 1 : -1;
        var combineDir = Eigen1.TryAdd(bi);
        if (combineDir == ChanDir.COMBINE)
            return false;
        Eigen2 = new Eigen(2, combineDir, bi);
        if (!ActualBreak())
            return Reset();
        Eigen1!.UpdateFx(Eigen0!, Eigen2!, allowTopEqual);
        var fx = Eigen1.Fx;
        var isFx = (DIR.IsUp() && fx == ChanFX.TOP) || (DIR.IsDown() && fx == ChanFX.BOTTOM);
        return isFx || Reset();
    }

    private bool ActualBreak()
    {
        if (Eigen2 == null || Eigen1 == null) throw new InvalidOperationException();
        if ((DIR.IsUp() && Eigen2.Low < Eigen1[^1].Low) || (DIR.IsDown() && Eigen2.High > Eigen1[^1].High))
            return true;
        if (Eigen2.Count != 1) throw new InvalidOperationException();
        var ele2Bi = Eigen2[0];
        if (ele2Bi.Next != null && ele2Bi.Next.Next != null)
        {
            if (ele2Bi.DIR.IsDown() && ele2Bi.Next.Next.Low < ele2Bi.Low)
            {
                LastEvidenceBi = ele2Bi.Next.Next;
                return true;
            }
            else if (ele2Bi.DIR.IsUp() && ele2Bi.Next.Next.High > ele2Bi.High)
            {
                LastEvidenceBi = ele2Bi.Next.Next;
                return true;
            }
        }

        return false;
    }

    public bool? CanBeEnd(List<Bi.Bi> biLst)
    {
        if (Eigen1 == null) throw new InvalidOperationException();
        if (Eigen1.Gap)
        {
            if (Eigen0 == null) throw new InvalidOperationException();
            int endBiIdx = GetPeakBiIdx();
            return FindRevertFx(biLst, endBiIdx + 2);
        }

        return true;
    }

    public int GetPeakBiIdx()
    {
        if (Eigen1 == null) throw new InvalidOperationException();
        return Eigen1.GetPeakBiIdx();
    }

    public void Clear()
    {
        Eigen0 = null;
        Eigen1 = null;
        Eigen2 = null;
        Lst.Clear();
    }

    public bool AllBiIsSure()
    {
        return true;
        // if (LastEvidenceBi == null) throw new InvalidOperationException();
        // return Lst.All(bi => bi.IsSure) && LastEvidenceBi.IsSure;
    }

    private bool Reset()
    {
        var biTmpList = Lst.Skip(1).ToList();
        Clear();
        return biTmpList.Any(Add);
    }

    private bool? FindRevertFx(List<Bi.Bi> biList, int beginIdx)
    {
        var firstBiDir = biList[beginIdx].DIR;
        var eigenFx = new EigenFx(firstBiDir.RevertDir());
        for (int i = beginIdx; i < biList.Count; i += 2)
        {
            var bi = biList[i];
            if (eigenFx.Add(bi))
            {
                while (true)
                {
                    var test = eigenFx.CanBeEnd(biList);
                    if (test == true || test == null)
                    {
                        LastEvidenceBi = bi;
                        return test;
                    }
                    else if (!eigenFx.Reset())
                    {
                        break;
                    }
                }
            }
        }

        return null;
    }
}