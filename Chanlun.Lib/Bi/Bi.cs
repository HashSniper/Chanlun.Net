using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.KLine;

namespace Chanlun.Lib.Bi
{
    public class Bi : ChanNode<Bi>
    {
        public Bi(int idx, KLineGroup startKLine, KLineGroup endKLine, bool isSure = true) 
        {
            Idx = idx;
            IsSure = isSure;
            StartKLine = startKLine;
            EndKLine = endKLine;
            SureEndKLines = new List<KLineGroup>();
        }

        public int Idx { get; }
        
        public ChanDir DIR => StartKLine.FX == ChanFX.TOP ? ChanDir.DOWN : ChanDir.UP;

        public KLineGroup StartKLine { get; set; }
        
        public KLineGroup EndKLine { get; set; }

        public bool? IsSure { get; set; }

        public List<KLineGroup> SureEndKLines { get; set; }

    }
}
