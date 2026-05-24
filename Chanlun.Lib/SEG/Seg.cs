using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.SEG;

/// <summary>
/// 线段相关
/// </summary>
public class Seg : ChanNode<Seg>
{
    public int Idx { get; }
    /// <summary>构成线段的所有笔</summary>
    public List<Bi.Bi> Bis { get; } = new();

    /// <summary>线段方向（与第一笔同向）</summary>
    public ChanDir DIR => Bis.Count > 0 ? Bis[0].DIR : ChanDir.UP;

    /// <summary>线段最高点</summary>
    public float High => Bis.Count > 0 ? Bis.Max(b => b.High) : 0;

    /// <summary>线段最低点</summary>
    public float Low => Bis.Count > 0 ? Bis.Min(b => b.Low) : 0;
    
    /// <summary>笔的数量</summary>
    public int Count => Bis.Count;

    public Seg(IEnumerable<Bi.Bi> bis,int idx)
    {
        Bis.AddRange(bis);
        Idx = idx;
    }

    /// <summary>
    /// 提取特征序列：与线段方向相反的笔
    /// 向上线段 → 所有向下笔；向下线段 → 所有向上笔
    /// </summary>
    public List<Bi.Bi> GetFeatureSequence()
    {
        return Bis.Where(b => b.DIR != DIR).ToList();
    }

    public override string ToString()
    {
        return $"[{nameof(Seg)}] {DIR} Idx={Idx} Count={Count} H={High} L={Low}";
    }
}