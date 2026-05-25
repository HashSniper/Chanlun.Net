using Chanlun.Lib.Bi;
using Chanlun.Lib.KLine;
using Chanlun.Lib.SEG;

namespace Chanlun.Lib;

public class ChanCalculateResult
{
    public ChanKLineList LineList { get; set; }
    
    public BiList BiList { get; set; }
    
    public List<Seg> SegList { get; set; }
}