using Chan.Lib.Common;
using Chan.Lib.Bis;

namespace Chan.Lib.Seg;

public class DefaultSegmentList : SegmentListBase
{
    public DefaultSegmentList(SegmentConfig segConfig = null!, SEG_TYPE lv = SEG_TYPE.BI) : base(segConfig ?? new SegmentConfig(), lv)
    {
    }

    public override void Update(IReadOnlyList<IChanLine> biLst)
    {
        // deprecated implementation placeholder
        throw new NotImplementedException("SegListDef is deprecated");
    }
}
