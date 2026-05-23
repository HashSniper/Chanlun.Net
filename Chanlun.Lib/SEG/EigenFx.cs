using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.SEG;

/// <summary>
/// 特征值线段组成的分型
/// </summary>
public class EigenFx
{
    public EigenFx(ChanDir dir)
    {
        Dir = dir;
        Eigens = new Eigen[3];
        BiList = [];
    }
    
    public Eigen?[]  Eigens { get; private set; }

    public ChanDir Dir { get; }

    public HashSet<Bi.Bi> BiList { get; }

    public Bi.Bi LastEvidenceBi { get; set; }

    public void Clear()
    {
        Eigens = new Eigen[3];
        BiList.Clear();
    }
}