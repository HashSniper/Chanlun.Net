using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.KLine
{
    public class KLineUnit(int idx) : ChanNode<KLineUnit>(idx)
    {
        public DateTime Time { get; set; }
    }
}