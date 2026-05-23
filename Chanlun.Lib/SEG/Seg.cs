using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.SEG;

/// <summary>
/// 线段相关
/// </summary>
public class Seg : ChanNode<Seg>
{

    public int Idx { get; set; }
    public Bi.Bi StartBi { get; set; }
    public Bi.Bi EndBi { get; set; }
    /// <summary>
    /// 一根线段内只可能有一个特征分型
    /// </summary>
    public EigenFx EigenFx { get; set; }
    public bool IsSure { get; set; }
    public ChanDir Dir { get; set; }
    
}