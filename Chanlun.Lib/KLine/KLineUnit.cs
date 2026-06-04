using Chanlun.Lib.ChanCommon;
using Skender.Stock.Indicators;

namespace Chanlun.Lib.KLine
{
    public class KLineUnit(int idx) : ChanNode<KLineUnit>(idx)
    {
        public DateTime Time { get; set; }
        
        public float Open { get; set; }
        
        public float Close { get; set; }

        public MacdResult MACD { get; set; }

        public BollingerBandsResult Boll { get; set; }

        public override string ToString() => $"Unit|{Idx}|{Time}";
    }
    
}