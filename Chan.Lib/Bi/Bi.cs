using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.Combiner;
using Chan.Lib.KLines;
using Chan.Lib.Seg;
using Chan.Lib.BuySellPoints;

namespace Chan.Lib.Bis;

public class Bi : IBiLine
{
    private KLine _beginKlc;
    private KLine _endKlc;
    private BI_DIR _dir;
    private int _idx;
    private BI_TYPE _type = BI_TYPE.STRICT;
    private bool _isSure;
    private List<KLine> _sureEnd = new();
    private int? _segIdx;
    public Segment? ParentSeg { get; set; }
    public BuySellPoint? Bsp { get; set; }
    public Bi? Next { get; set; }
    public Bi? Pre { get; set; }
    IBiLine? IBiLine.Pre => Pre;
    IBiLine? IBiLine.Next => Next;
    private readonly Dictionary<string, object> _memoizeCache = new();

    public KLine BeginKlc => _beginKlc;
    public KLine EndKlc => _endKlc;
    public BI_DIR Dir => _dir;
    public int Idx => _idx;
    public BI_TYPE Type => _type;
    public bool IsSure => _isSure;
    public List<KLine> SureEnd => _sureEnd;
    public int? SegIdx => _segIdx;

    public Bi(KLine beginKlc, KLine endKlc, int idx, bool isSure)
    {
        _idx = idx;
        _isSure = isSure;
        Set(beginKlc, endKlc);
    }

    public void CleanCache() => _memoizeCache.Clear();

    public void SetSegIdx(int idx) => _segIdx = idx;

    public override string ToString() => $"{Dir}|{BeginKlc} ~ {EndKlc}";

    public void Check()
    {
        try
        {
            if (IsDown())
            {
                if (!(BeginKlc.High > EndKlc.Low))
                    throw new InvalidOperationException();
            }
            else
            {
                if (!(BeginKlc.Low < EndKlc.High))
                    throw new InvalidOperationException();
            }
        }
        catch (Exception)
        {
            throw new ChanException($"{_idx}:{BeginKlc[0].Time}~{EndKlc[^1].Time}笔的方向和收尾位置不一致!", ErrCode.BI_ERR);
        }
    }

    public void Set(KLine beginKlc, KLine endKlc)
    {
        _beginKlc = beginKlc;
        _endKlc = endKlc;
        _dir = beginKlc.Fx switch
        {
            FX_TYPE.BOTTOM => BI_DIR.UP,
            FX_TYPE.TOP => BI_DIR.DOWN,
            _ => throw new ChanException("ERROR DIRECTION when creating bi", ErrCode.BI_ERR)
        };
        Check();
        CleanCache();
    }

    public float GetBeginVal()
    {
        var res = IsUp() ? BeginKlc.Low : BeginKlc.High;
        return res;
    }

    public float GetEndVal()
    {
        var res = IsUp() ? EndKlc.High : EndKlc.Low;
        return res;
    }

    public KLineUnit GetBeginKlu()
    {
        var res = IsUp() ? BeginKlc.GetLowPeakKlu() : BeginKlc.GetHighPeakKlu();
        return res;
    }

    public KLineUnit GetEndKlu()
    {
        var res = IsUp() ? EndKlc.GetHighPeakKlu() : EndKlc.GetLowPeakKlu();
        return res;
    }

    public float Amp()
    {
        var res = Math.Abs(GetEndVal() - GetBeginVal());
        return res;
    }

    public int GetKluCnt()
    {
        var res = GetEndKlu().Idx - GetBeginKlu().Idx + 1;
        return res;
    }

    public int GetKlcCnt()
    {
        if (EndKlc.Idx != GetEndKlu().Klc.Idx || BeginKlc.Idx != GetBeginKlu().Klc.Idx)
            throw new InvalidOperationException();
        var res = EndKlc.Idx - BeginKlc.Idx + 1;
        return res;
    }

    public float High()
    {
        var res = IsUp() ? EndKlc.High : BeginKlc.High;
        return res;
    }

    public float Low()
    {
        var res = IsUp() ? BeginKlc.Low : EndKlc.Low;
        return res;
    }

    public double Mid()
    {
        var res = (High() + Low()) / 2;
        return res;
    }

    public bool IsDown()
    {
        var res = Dir == BI_DIR.DOWN;
        return res;
    }

    public bool IsUp()
    {
        var res = Dir == BI_DIR.UP;
        return res;
    }

    public void UpdateVirtualEnd(KLine newKlc)
    {
        AppendSureEnd(EndKlc);
        UpdateNewEnd(newKlc);
        _isSure = false;
    }

    public void RestoreFromVirtualEnd(KLine sureEnd)
    {
        _isSure = true;
        UpdateNewEnd(sureEnd);
        _sureEnd = new List<KLine>();
    }

    public void AppendSureEnd(KLine klc) => _sureEnd.Add(klc);

    public void UpdateNewEnd(KLine newKlc)
    {
        _endKlc = newKlc;
        Check();
        CleanCache();
    }

    public double CalMacdMetric(MACD_ALGO macdAlgo, bool isReverse)
    {
        return macdAlgo switch
        {
            MACD_ALGO.AREA => CalMacdHalf(isReverse),
            MACD_ALGO.PEAK => CalMacdPeak(),
            MACD_ALGO.FULL_AREA => CalMacdArea(),
            MACD_ALGO.DIFF => CalMacdDiff(),
            MACD_ALGO.SLOPE => CalMacdSlope(),
            MACD_ALGO.AMP => CalMacdAmp(),
            MACD_ALGO.AMOUNT => CalMacdTradeMetric(DATA_FIELD.FIELD_TURNOVER, calAvg: false),
            MACD_ALGO.VOLUMN => CalMacdTradeMetric(DATA_FIELD.FIELD_VOLUME, calAvg: false),
            MACD_ALGO.VOLUMN_AVG => CalMacdTradeMetric(DATA_FIELD.FIELD_VOLUME, calAvg: true),
            MACD_ALGO.AMOUNT_AVG => CalMacdTradeMetric(DATA_FIELD.FIELD_TURNOVER, calAvg: true),
            MACD_ALGO.TURNRATE_AVG => CalMacdTradeMetric(DATA_FIELD.FIELD_TURNRATE, calAvg: true),
            MACD_ALGO.RSI => CalRsi(),
            _ => throw new ChanException($"unsupport macd_algo={macdAlgo}", ErrCode.PARA_ERROR)
        };
    }

    private double CalRsi()
    {
        var rsiLst = new List<double>();
        foreach (var klc in KlcLst())
        {
            foreach (var klu in klc.Lst)
            {
                if (klu.Rsi.HasValue)
                    rsiLst.Add(klu.Rsi.Value);
            }
        }
        double res = IsDown() ? 10000.0 / (rsiLst.Min() + 1e-7) : rsiLst.Max();
        return res;
    }

    private double CalMacdArea()
    {
        double s = 1e-7;
        var beginKlu = GetBeginKlu();
        var endKlu = GetEndKlu();
        foreach (var klc in KlcLst())
        {
            foreach (var klu in klc.Lst)
            {
                if (klu.Idx < beginKlu.Idx || klu.Idx > endKlu.Idx) continue;
                if (klu.Macd == null) continue;
                if ((IsDown() && klu.Macd.Macd < 0) || (IsUp() && klu.Macd.Macd > 0))
                    s += Math.Abs(klu.Macd.Macd);
            }
        }
        return s;
    }

    private double CalMacdPeak()
    {
        double peak = 1e-7;
        foreach (var klc in KlcLst())
        {
            foreach (var klu in klc.Lst)
            {
                if (klu.Macd == null) continue;
                if (Math.Abs(klu.Macd.Macd) > peak)
                {
                    if (IsDown() && klu.Macd.Macd < 0)
                        peak = Math.Abs(klu.Macd.Macd);
                    else if (IsUp() && klu.Macd.Macd > 0)
                        peak = Math.Abs(klu.Macd.Macd);
                }
            }
        }
        return peak;
    }

    private double CalMacdHalf(bool isReverse)
    {
        return isReverse ? CalMacdHalfReverse() : CalMacdHalfObverse();
    }

    private double CalMacdHalfObverse()
    {
        double s = 1e-7;
        var beginKlu = GetBeginKlu();
        double peakMacd = beginKlu.Macd?.Macd ?? 0;
        foreach (var klc in KlcLst())
        {
            foreach (var klu in klc.Lst)
            {
                if (klu.Idx < beginKlu.Idx) continue;
                if (klu.Macd == null) continue;
                if (klu.Macd.Macd * peakMacd > 0)
                    s += Math.Abs(klu.Macd.Macd);
                else
                    goto end;
            }
        }
    end:
        return s;
    }

    private double CalMacdHalfReverse()
    {
        double s = 1e-7;
        var beginKlu = GetEndKlu();
        double peakMacd = beginKlu.Macd?.Macd ?? 0;
        foreach (var klc in KlcLstRe())
        {
            for (int i = klc.Lst.Count - 1; i >= 0; i--)
            {
                var klu = klc.Lst[i];
                if (klu.Idx > beginKlu.Idx) continue;
                if (klu.Macd == null) continue;
                if (klu.Macd.Macd * peakMacd > 0)
                    s += Math.Abs(klu.Macd.Macd);
                else
                    goto end;
            }
        }
    end:
        return s;
    }

    private double CalMacdDiff()
    {
        double max = double.NegativeInfinity, min = double.PositiveInfinity;
        foreach (var klc in KlcLst())
        {
            foreach (var klu in klc.Lst)
            {
                if (klu.Macd == null) continue;
                var macd = klu.Macd.Macd;
                if (macd > max) max = macd;
                if (macd < min) min = macd;
            }
        }
        var res = max - min;
        return res;
    }

    private double CalMacdSlope()
    {
        var beginKlu = GetBeginKlu();
        var endKlu = GetEndKlu();
        double res;
        if (IsUp())
            res = (endKlu.High - beginKlu.Low) / endKlu.High / (endKlu.Idx - beginKlu.Idx + 1);
        else
            res = (beginKlu.High - endKlu.Low) / beginKlu.High / (endKlu.Idx - beginKlu.Idx + 1);
        return res;
    }

    private double CalMacdAmp()
    {
        var beginKlu = GetBeginKlu();
        var endKlu = GetEndKlu();
        double res;
        if (IsDown())
            res = (beginKlu.High - endKlu.Low) / beginKlu.High;
        else
            res = (endKlu.High - beginKlu.Low) / beginKlu.Low;
        return res;
    }

    private double CalMacdTradeMetric(string metric, bool calAvg)
    {
        double s = 0;
        int cnt = 0;
        foreach (var klc in KlcLst())
        {
            foreach (var klu in klc.Lst)
            {
                if (!klu.TradeInfo.Metric.TryGetValue(metric, out var metricRes) || !metricRes.HasValue)
                    return 0.0;
                s += metricRes.Value;
                cnt++;
            }
        }
        return calAvg ? s / cnt : s;
    }

    private IEnumerable<KLine> KlcLst()
    {
        var klc = BeginKlc;
        while (true)
        {
            yield return klc;
            klc = klc.Next as KLine;
            if (klc == null || klc.Idx > EndKlc.Idx) break;
        }
    }

    private IEnumerable<KLine> KlcLstRe()
    {
        var klc = EndKlc;
        while (true)
        {
            yield return klc;
            klc = klc.Pre as KLine;
            if (klc == null || klc.Idx < BeginKlc.Idx) break;
        }
    }

    object ICombineSource.TimeBegin => BeginKlc.Idx;
    object ICombineSource.TimeEnd => EndKlc.Idx;
    float ICombineSource.CombineHigh => High();
    float ICombineSource.CombineLow => Low();
}
