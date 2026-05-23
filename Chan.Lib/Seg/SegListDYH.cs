using Chan.Lib.Common;
using Chan.Lib.Bis;

namespace Chan.Lib.Seg;

public class DyhSegmentList : SegmentListBase
{
    public DyhSegmentList(SegmentConfig segConfig = null!, SEG_TYPE lv = SEG_TYPE.BI) : base(segConfig ?? new SegmentConfig(), lv)
    {
    }

    public override void Update(IReadOnlyList<IBiLine> biLst)
    {
        // deprecated implementation placeholder
        throw new NotImplementedException("SegListDYH is deprecated");
    }
}
