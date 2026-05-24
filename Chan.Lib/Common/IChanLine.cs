using Chan.Lib.Common;
using Chan.Lib.Combiner;
using Chan.Lib.KLines;
using Chan.Lib.BuySellPoints;
using Chan.Lib.Seg;

namespace Chan.Lib.Bis;

public interface IChanLine : ICombineSource
{
    bool IsSure { get; }
    CHAN_DIR Dir { get; }
    int Idx { get; }
    bool IsUp();
    bool IsDown();
    float High();
    float Low();
    float GetBeginVal();
    float GetEndVal();
    float Amp();
    double CalMacdMetric(MACD_ALGO macdAlgo, bool isReverse);
    Segment? ParentSeg { get; set; }
    BuySellPoint? Bsp { get; set; }
    public KLine BeginKlc { get; }
    public KLine EndKlc { get; }
    KLineUnit GetBeginKlu();
    KLineUnit GetEndKlu();
    IChanLine? Pre { get; }
    IChanLine? Next { get; }
    int? SegIdx { get; }
}
