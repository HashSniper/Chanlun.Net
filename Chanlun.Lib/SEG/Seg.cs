using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;

namespace Chanlun.Lib.SEG;

/// <summary>
/// 线段相关
/// </summary>
public class Seg(int idx, Bi.Bi startBi, Bi.Bi endBi, bool isSure = true)
    : ChanNode<Seg>(idx), IDirectional
{
    /// <summary>构成线段的所有笔</summary>
    public List<Bi.Bi> Lst { get; } = new();

    /// <summary>线段方向</summary>
    public ChanDir DIR => EndBi.DIR;

    /// <summary>线段最高点</summary>
    public override float High => DIR.IsUp() ? EndBi.EndChanKLine.PeakUnit.High : StartBi.StartChanKLine.PeakUnit.High;

    /// <summary>线段最低点</summary>
    public override float Low => DIR.IsUp() ? StartBi.StartChanKLine.PeakUnit.Low : EndBi.EndChanKLine.PeakUnit.Low;

    public Bi.Bi StartBi { get; private set; } = startBi;
    public Bi.Bi EndBi { get; private set; } = endBi;

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
        return $"[{nameof(Seg)}] {DIR} Idx={Idx} Count={Lst.Count} H={High} L={Low}";
    }
}