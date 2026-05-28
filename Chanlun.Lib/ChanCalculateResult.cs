using Chanlun.Lib.Bi;
using Chanlun.Lib.KLine;
using Chanlun.Lib.SEG;
using Chanlun.Lib.Zs;

namespace Chanlun.Lib;

public class ChanCalculateResult
{
    public ChanKLineList LineList { get; set; }
    
    public BiList BiList { get; set; }
    
    public List<Seg> SegList { get; set; }
    
    public SegPivotList SegPivotList { get; set; }
}