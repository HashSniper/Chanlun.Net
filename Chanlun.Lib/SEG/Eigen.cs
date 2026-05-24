using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;

namespace Chanlun.Lib.SEG;

public class Eigen(int idx, ChanDir dir, Bi.Bi bi) : ChanNode<Eigen>(idx)
{
    private List<Bi.Bi> Lst { get; } = [bi];
    public ChanDir DIR { get; } = dir;
    public float High { get; private set; } = bi.High;
    public float Low { get; private set; } = bi.Low;
    public int Count => Lst.IsNullOrEmpty() ? 0 : Lst.Count;
    public bool Gap { get; set; } = false;

    public ChanFX Fx { get; private set; }

    public Bi.Bi this[int index] => Lst[index];

    public ChanDir TryAdd(Bi.Bi bi)
    {
        var dir = TestCombine(bi);
        if (dir == ChanDir.COMBINE)
        {
            Lst.Add(bi);
            if (DIR == ChanDir.UP)
            {
                High = Math.Max(High, bi.High);
                Low = Math.Max(Low, bi.Low);
            }
            else if (DIR == ChanDir.DOWN)
            {
                High = Math.Min(High, bi.High);
                Low = Math.Min(Low, bi.Low);
            }
        }

        return dir;
    }

    public void UpdateFx(Eigen pre, Eigen next)
    {
        Pre = pre;
        Next = next;
        Pre.Next = this;
        Next.Pre = this;

        if (pre.High < High && next.High < High && pre.Low < Low && next.Low < Low)
        {
            Fx = ChanFX.TOP;
        }
        else if (pre.High > High && next.High > High && pre.Low > Low && next.Low > Low)
        {
            Fx = ChanFX.BOTTOM;
        }

        if ((Fx == ChanFX.TOP && pre.High < Low) || (Fx == ChanFX.BOTTOM && pre.Low > High))
            Gap = true;
    }

    public override string ToString() => $"{this[0].Idx}~{this[^1].Idx} gap={Gap} fx={Fx}";

    public int GetPeakBiIdx()
    {
        if (Fx == ChanFX.UNKNOWN) throw new InvalidOperationException();
        var biDir = this[0].DIR;

        if (biDir == ChanDir.UP)
            return GetLowPeakBi().Idx - 1;
        else
            return GetHighPeakBi().Idx - 1;
    }

    private ChanDir TestCombine(Bi.Bi item)
    {
        if (High > item.High && Low > item.Low)
            return ChanDir.DOWN;
        if (High < item.High && Low < item.Low)
            return ChanDir.UP;

        return ChanDir.COMBINE;
    }


    private Bi.Bi GetLowPeakBi()
    {
        return Lst.AsEnumerable().First(p => p.Low == Low);
    }

    private Bi.Bi GetHighPeakBi()
    {
        return Lst.AsEnumerable().First(p => p.High == High);
    }
}