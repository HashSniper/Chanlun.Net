using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.Zs;

public abstract class PivotBase<TNode, TSegment>(int idx) : ChanNode<TNode>(idx)
    where TNode : PivotBase<TNode, TSegment>
    where TSegment : ChanNode<TSegment>, IDirectional
{
    public TSegment InSegment { get; set; } = null!;

    public TSegment OutSegment { get; set; } = null!;

    public ChanDir DIR { get; set; }

    /// <summary>
    /// 构成中枢的所有线段
    /// </summary>
    public List<TSegment> Segments { get; private set; } = [];

    /// <summary>
    /// 中枢高点
    /// </summary>
    public float ZG { get; private set; }

    /// <summary>
    /// 中枢低点
    /// </summary>
    public float ZD { get; private set; }

    public float GG { get; private set; } // 中枢震荡最高点

    public float DD { get; private set; } // 中枢震荡最低点

    public override float Low => ZD;

    public override float High => ZG;

    public bool IsClosed { get; private set; }

    public int Level { get; set; } = 1;

    public bool TryInitialize(TSegment entry, TSegment s1, TSegment s2, TSegment s3)
    {
        var maxLow = Math.Max(s1.Low, Math.Max(s2.Low, s3.Low));
        var minHigh = Math.Min(s1.High, Math.Min(s2.High, s3.High));

        if (maxLow > minHigh)
        {
            return false;
        }

        ZD = maxLow;
        ZG = minHigh;
        GG = Math.Max(s1.High, Math.Max(s2.High, s3.High));
        DD = Math.Min(s1.Low, Math.Min(s2.Low, s3.Low));
        DIR = entry.DIR;

        InSegment = entry;
        Segments.AddRange([s1, s2, s3]);

        return true;
    }

    // 处理新线段（包含中枢延伸与 9 段扩展逻辑）
    public bool ProcessNextSegment(TSegment s)
    {
        if (IsClosed)
        {
            return false;
        }

        // 只要与 [ZD, ZG] 还有重叠，就属于中枢震荡延伸
        // 构成第三类买卖点，此时设置退出
        if (!IsOverlap(s) || (s.Next != null && !IsOverlap(s.Next)))
        {
            // 无法延伸，说明该线段脱离了中枢，确认为退出段
            OutSegment = s;
            IsClosed = true;
            return false;
        }

        Segments.Add(s);
        GG = Math.Max(GG, s.High);
        DD = Math.Min(DD, s.Low);

        // 9 段升级逻辑不变
        if (Segments.Count == 9)
        {
            Level++;
        }

        return true;
    }

    private bool IsOverlap(TSegment s)
    {
        return s.Low <= ZG && s.High >= ZD;
    }
}
