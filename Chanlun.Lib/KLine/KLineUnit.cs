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

        public KLineUnitIndicator UnitIndicator { get; set; } = new();

        public override string ToString() => $"Unit|{Idx}|{Time}";
    }

    public class KLineUnitIndicator
    {
        public MacdResult? MACD { get; set; }

        public BollingerBandsResult? Boll { get; set; }

        public decimal ChanEnergy { get; set; }

        public bool? IsBuy1 { get; set; }
        public bool? IsBuy2 { get; set; }
        public bool? IsBuy3 { get; set; }
        public bool? IsSell1 { get; set; }
        public bool? IsSell2 { get; set; }
        public bool? IsSell3 { get; set; }
    }
}