using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.SEG;

public class EigenFx(ChanDir dir)
{
    public Eigen Eigen1 { get; set; } = new();
    public Eigen Eigen2 { get; set; } = new();
    public Eigen Eigen3 { get; set; } = new();
    public ChanDir DIR { get;  } = dir;
}