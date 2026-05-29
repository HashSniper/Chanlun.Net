using Chanlun.Lib.SEG;

namespace Chanlun.Lib.Zs;

public class SegPivotList : PivotListBase<SegPivot, Seg>
{
    public SegPivotList() : base(idx => new SegPivot(idx))
    {
    }
}
