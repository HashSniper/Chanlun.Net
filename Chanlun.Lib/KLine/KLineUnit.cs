using Chanlun.Lib.ChanCommon;

namespace Chanlun.Lib.KLine
{
    public class KLineUnit(int idx) : ChanNode<KLineUnit>
    {
        public int Idx { get; } = idx;

        public DateTime Time { get; set; }
    }
}