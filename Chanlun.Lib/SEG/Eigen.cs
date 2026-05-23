using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.SEG;

/// <summary>
/// 特征笔或者线段
/// </summary>
public class Eigen : ChanNode<Eigen>
{
    public Eigen(ChanDir dir, Bi.Bi bi)
    {
        Dir = dir;
        BiList = [bi];
        High = bi.High;
        Low = bi.Low;
        PeakBi = bi;
    }

    public ChanDir Dir { get; }

    private ChanFX? _fx;
    public ChanFX? FX
    {
        get
        {
            if (!_fx.HasValue && Pre != null && Next != null)
            {
                if (Pre.High < High && Next.High < High && Pre.Low < Low && Next.Low < Low)
                {
                    _fx = ChanFX.TOP;
                }
                else if (Pre.High > High && Next.High > High && Pre.Low > Low && Next.Low > Low)
                {
                    _fx = ChanFX.BOTTOM;
                }
            }

            return _fx;
        }
    }
    
    /// <summary>
    /// 可能存在包含关系的笔，所以会有多个
    /// </summary>
    public HashSet<Bi.Bi> BiList{ get;}

    public Bi.Bi PeakBi { get; private set; }

    public void AddBi(Bi.Bi bi)
    {
        if (BiList.Add(bi))
        {
            if (Dir == ChanDir.UP)
            {
                High = Math.Max(High, bi.High);
                Low = Math.Max(Low, bi.Low);
            }
            else if (Dir == ChanDir.DOWN && (bi.High != bi.Low || bi.Low != Low))
            {
                High = Math.Min(High, bi.High);
                Low = Math.Min(Low, bi.Low);
            }
            PeakBi= Dir == ChanDir.UP
                ? BiList.First(p => p.High == High)
                : BiList.First(p => p.Low == Low);
        }
    }

    public bool Gap { get; set; }
}