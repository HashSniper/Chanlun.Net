using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.Combiner;
using Chan.Lib.Indicators;

namespace Chan.Lib.KLines;

public class KLineUnit : ICombineSource
{
    public KL_TYPE? KlType { get; set; }
    public DateTime Time { get; }
    public float Close { get; }
    public float Open { get; }
    public float High { get; }
    public float Low { get; }

    public TradeInfo TradeInfo { get; }
    public DemarkIndex Demark { get; set; } = new();

    public List<KLineUnit> SubKlList { get; } = new();
    public KLineUnit? SupKl { get; private set; }

    private KLine? _klc;
    public KLine Klc
    {
        get
        {
            if (_klc == null) throw new InvalidOperationException("klc is null");
            return _klc;
        }
    }

    public void SetKlc(KLine klc) => _klc = klc;

    public MacdItem? Macd { get; set; }
    public BollMetric? Boll { get; set; }
    public double? Rsi { get; set; }
    public KdjItem? Kdj { get; set; }

    public Dictionary<TREND_TYPE, Dictionary<int, double>> Trend { get; } = new();

    public int LimitFlag { get; set; } = 0;
    public KLineUnit? Pre { get; private set; }
    public KLineUnit? Next { get; private set; }

    private int _idx = -1;
    public int Idx => _idx;

    public KLineUnit(DateTime time, float high, float low,int idx)
    {
        Time = time;
        Close = high;
        Open = low;
        High = high;
        Low = low;
        TradeInfo = new TradeInfo(new Dictionary<string, object>());
        _idx = idx;
    }

    public override string ToString()
    {
        return $"{Idx}:{Time}/{KlType} open={Open} close={Close} high={High} low={Low} {TradeInfo}";
    }

    private void Check(bool autofix)
    {
        double minVal = Math.Min(Math.Min(Low, Open), Math.Min(High, Close));
        double maxVal = Math.Max(Math.Max(Low, Open), Math.Max(High, Close));
        if (Low > minVal)
        {
            if (autofix)
            { /* Low = minVal; */ }
            else
                throw new ChanException($"{Time} low price={Low} is not min of [low={Low}, open={Open}, high={High}, close={Close}]", ErrCode.KL_DATA_INVALID);
        }
        if (High < maxVal)
        {
            if (autofix)
            { /* High = maxVal; */ }
            else
                throw new ChanException($"{Time} high price={High} is not max of [low={Low}, open={Open}, high={High}, close={Close}]", ErrCode.KL_DATA_INVALID);
        }
    }

    public void AddChildren(KLineUnit child) => SubKlList.Add(child);
    public void SetParent(KLineUnit parent) => SupKl = parent;
    public IEnumerable<KLineUnit> GetChildren() => SubKlList;

    public void SetMetric(List<object> metricModelLst)
    {
        foreach (var metricModel in metricModelLst)
        {
            switch (metricModel)
            {
                case Macd macd:
                    Macd = macd.Add(Close);
                    break;
                case TrendModel tm:
                    if (!Trend.ContainsKey(tm.Type))
                        Trend[tm.Type] = new Dictionary<int, double>();
                    Trend[tm.Type][tm.T] = tm.Add(Close);
                    break;
                case BollModel bm:
                    Boll = bm.Add(Close);
                    break;
                case DemarkEngine de:
                    Demark = de.Update(Idx, Close, High, Low);
                    break;
                case RSI rsi:
                    Rsi = rsi.Add(Close);
                    break;
                case KDJ kdj:
                    Kdj = kdj.Add(High, Low, Close);
                    break;
            }
        }
    }

    public KLine GetParentKlc()
    {
        if (SupKl == null) throw new InvalidOperationException("sup_kl is null");
        return SupKl.Klc;
    }

    public bool IncludeSubLvTime(string subLvT)
    {
        if (Time.ToString() == subLvT) return true;
        foreach (var subKlu in SubKlList)
        {
            if (subKlu.Time.ToString() == subLvT) return true;
            if (subKlu.IncludeSubLvTime(subLvT)) return true;
        }
        return false;
    }

    public void SetPreKlu(KLineUnit? preKlu)
    {
        if (preKlu == null) return;
        preKlu.Next = this;
        Pre = preKlu;
    }

    object ICombineSource.TimeBegin => Time;
    object ICombineSource.TimeEnd => Time;
    float ICombineSource.CombineHigh => High;
    float ICombineSource.CombineLow => Low;
}
