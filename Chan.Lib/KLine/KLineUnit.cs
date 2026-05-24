using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.Combiner;
using Chan.Lib.Indicators;

namespace Chan.Lib.KLines;

public class KLineUnit(DateTime time, float high, float low, int idx)
    : ICombineSource
{
    public KL_TYPE? KlType { get; }
    public DateTime Time { get; } = time;
    public float High { get; } = high;
    public float Low { get; } = low;

    public TradeInfo TradeInfo { get; } = new(new Dictionary<string, object>());
    public DemarkIndex Demark { get; set; } = new();
    
    public MacdItem? Macd { get; set; }
    public BollMetric? Boll { get; set; }
    public double? Rsi { get; set; }
    public KdjItem? Kdj { get; set; }

    public int Idx { get; } = idx;

    public override string ToString()
    {
        return $"{Idx}:{Time}/{KlType} high={High} low={Low}";
    }
    
    DateTime ICombineSource.TimeBegin => Time;
    DateTime ICombineSource.TimeEnd => Time;
    float ICombineSource.CombineHigh => High;
    float ICombineSource.CombineLow => Low;
}
