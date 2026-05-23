using Chan.Lib.Common;
using Chan.Lib.Combiner;
using Chan.Lib.KLines;
using Chan.Lib.BuySellPoints;
using Chan.Lib.Seg;

namespace Chan.Lib.Bis;

public interface IBiLine : ICombineSource
{
    bool IsSure { get; }
    BI_DIR Dir { get; }
    int Idx { get; }
    bool IsUp();
    bool IsDown();
    float High();
    float Low();
    float GetBeginVal();
    float GetEndVal();
    KLineUnit GetBeginKlu();
    KLineUnit GetEndKlu();
    float Amp();
    double CalMacdMetric(MACD_ALGO macdAlgo, bool isReverse);
    int? SegIdx { get; }
    void SetSegIdx(int idx);
    Segment? ParentSeg { get; set; }
    BuySellPoint? Bsp { get; set; }
    IBiLine? Pre { get; }
    IBiLine? Next { get; }
}
