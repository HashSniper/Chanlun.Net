using Chanlun.Lib.ChanCommon;
using Skender.Stock.Indicators;

namespace Chanlun.Lib.KLine
{
    public class KLineUnit(int idx) : ChanNode<KLineUnit>(idx)
    {
        public DateTime Time { get; set; }
        
        public decimal Open { get; set; }
        
        public decimal Close { get; set; }
        
        public decimal Volume { get; set; }
        
        public decimal Amount { get; set; }

        public MacdResult MACD { get; set; }

        public BollingerBandsResult Boll { get; set; }

        public decimal CalIndicator { get; set; }

        public override string ToString() => $"Unit|{Idx}|{Time}";
    }
    
}