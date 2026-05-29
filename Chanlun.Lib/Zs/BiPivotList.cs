namespace Chanlun.Lib.Zs;

public class BiPivotList : PivotListBase<BiPivot, Bi.Bi>
{
    public BiPivotList() : base(idx => new BiPivot(idx))
    {
    }
}
