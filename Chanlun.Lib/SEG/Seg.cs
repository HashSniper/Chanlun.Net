using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.SEG;

/// <summary>
/// 线段相关
/// </summary>
public class Seg(int idx, Bi.Bi startBi, Bi.Bi endBi, bool isSure = true)
    : ChanNode<Seg>(idx)
{
    /// <summary>构成线段的所有笔</summary>
    public List<Bi.Bi> Lst { get; } = new();

    /// <summary>线段方向（与第一笔同向）</summary>
    public ChanDir DIR => Lst.Count > 0 ? Lst[0].DIR : ChanDir.UP;

    /// <summary>线段最高点</summary>
    public float High => Lst.Count > 0 ? Lst.Max(b => b.High) : 0;

    /// <summary>线段最低点</summary>
    public float Low => Lst.Count > 0 ? Lst.Min(b => b.Low) : 0;
    
    /// <summary>笔的数量</summary>
    public int Count => Lst.Count;
    
    public Bi.Bi StartBi { get;  private set; } = startBi;
    public Bi.Bi EndBi { get; private set; } = endBi;
    public bool IsSure { get; set; } = isSure;
    public EigenFx? EigenFx { get; set; }

    public void UpdateBiList(List<Bi.Bi> biLst, int idx1, int idx2)
    {
        for (int biIdx = idx1; biIdx <= idx2; biIdx++)
        {
            Lst.Add(biLst[biIdx]);
        }
    }

    public override string ToString()
    {
        return $"[{nameof(Seg)}] {DIR} Idx={Idx} Count={Count} H={High} L={Low}";
    }
}