using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.SEG;

namespace Chanlun.Lib.Zs;
using Bi= Bi.Bi;

public class BiPivot(int idx) : ChanNode<BiPivot>(idx)
{
    public Bi InBi { get; set; }

    public Bi OutBi { get; set; }

    public ChanDir DIR { get; set; }

    /// <summary>
    /// 构成中枢的所有线段
    /// </summary>
    public List<Bi> BiList { get; private set; } = [];

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

    public bool TryInitialize(Bi entry, Bi b1, Bi b2, Bi b3)
    {
        var maxLow = Math.Max(b1.Low, Math.Max(b2.Low, b3.Low));
        var minHigh = Math.Min(b1.High, Math.Min(b2.High, b3.High));

        if (maxLow > minHigh)
        {
            return false;
        }

        ZD = maxLow;
        ZG = minHigh;
        GG = Math.Max(b1.High, Math.Max(b2.High, b3.High));
        DD = Math.Min(b1.Low, Math.Min(b2.Low, b3.Low));
        DIR = entry.DIR;

        InBi = entry;
        BiList.AddRange([b1, b2, b3]);

        return true;
    }

    // 处理新线段（包含中枢延伸与 9 段扩展逻辑）
    public bool ProcessNextSegment(Bi bi)
    {
        if (IsClosed)
        {
            return false;
        }

        // 只要与 [ZD, ZG] 还有重叠，就属于中枢震荡延伸
        // 构成第三类买卖点，此时设置退出
        if (!IsOverlap(bi) || (bi.Next != null && !IsOverlap(bi.Next)))
        {
            // 无法延伸，说明该线段脱离了中枢，确认为退出段
            OutBi = bi;
            IsClosed = true;
            return false;
        }
        
        BiList.Add(bi);
        GG = Math.Max(GG, bi.High);
        DD = Math.Min(DD, bi.Low);

        // 9 段升级逻辑不变
        if (BiList.Count == 9)
        {
            Level++;
        }

        return true;
    }

    private bool IsOverlap(Bi s)
    {
        return s.Low <= ZG && s.High >= ZD;
    }
}
