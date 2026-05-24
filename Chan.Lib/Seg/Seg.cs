using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.Combiner;
using Chan.Lib.KLines;
using Chan.Lib.Bis;
using Chan.Lib.ZS;
using Chan.Lib.BuySellPoints;
using Chan.Lib.Indicators;

namespace Chan.Lib.Seg;

public class Segment : IChanLine
{
    public int Idx { get; }
    public IChanLine StartChan { get; }
    public IChanLine EndChan { get; private set; }
    public bool IsSure { get; set; }
    public CHAN_DIR Dir { get; }
    public List<Pivot> ZsLst { get; } = new();
    public EigenFeature? EigenFx { get; set; }
    public int? SegIdx { get; set; }
    public Segment? ParentSeg { get; set; }

    public Segment? Pre { get; set; }
    public Segment? Next { get; set; }
    IChanLine? IChanLine.Pre => Pre;
    IChanLine? IChanLine.Next => Next;
    public BuySellPoint? Bsp { get; set; }
    public KLine BeginKlc { get; }
    public KLine EndKlc { get; }
    public List<IChanLine> BiList { get; } = new();
    public string Reason { get; }
    public TrendLine? SupportTrendLine { get; set; }
    public TrendLine? ResistanceTrendLine { get; set; }
    public bool EleInsideIsSure { get; set; } = false;

    public Segment(int idx, IChanLine startChan, IChanLine endChan, bool isSure = true, CHAN_DIR? segDir = null, string reason = "normal")
    {
        Idx = idx;
        StartChan = startChan;
        EndChan = endChan;
        IsSure = isSure;
        Dir = segDir ?? endChan.Dir;
        Reason = reason;
        if (endChan.Idx - startChan.Idx < 2)
            IsSure = false;
        Check();
    }

    public void SetSegIdx(int idx) => SegIdx = idx;

    public void Check()
    {
        if (!IsSure) return;
        if (IsDown())
        {
            if (StartChan.GetBeginVal() < EndChan.GetEndVal())
                throw new ChanException($"下降线段起始点应该高于结束点! idx={Idx}", ErrCode.SEG_END_VALUE_ERR);
        }
        else if (StartChan.GetBeginVal() > EndChan.GetEndVal())
        {
            throw new ChanException($"上升线段起始点应该低于结束点! idx={Idx}", ErrCode.SEG_END_VALUE_ERR);
        }
        if (EndChan.Idx - StartChan.Idx < 2)
            throw new ChanException($"线段({StartChan.Idx}-{EndChan.Idx})长度不能小于2! idx={Idx}", ErrCode.SEG_LEN_ERR);
    }

    public override string ToString() => $"{StartChan.Idx}->{EndChan.Idx}: {Dir} {IsSure}";

    public void AddZs(Pivot zs) => ZsLst.Insert(0, zs);
    public void ClearZsLst() => ZsLst.Clear();

    public float CalKluSlope() => (GetEndVal() - GetBeginVal()) / (GetEndKlu().Idx - GetBeginKlu().Idx) / GetBeginVal();
    public float CalAmp() => (GetEndVal() - GetBeginVal()) / GetBeginVal();
    public int CalBiCnt() => EndChan.Idx - StartChan.Idx + 1;

    public float Low() => IsDown() ? EndChan.GetEndKlu().Low : StartChan.GetBeginKlu().Low;
    public float High() => IsUp() ? EndChan.GetEndKlu().High : StartChan.GetBeginKlu().High;
    public bool IsDown() => Dir == CHAN_DIR.DOWN;
    public bool IsUp() => Dir == CHAN_DIR.UP;
    public float GetEndVal() => EndChan.GetEndVal();
    public float GetBeginVal() => StartChan.GetBeginVal();
    public float Amp() => Math.Abs(GetEndVal() - GetBeginVal());
    public KLineUnit GetEndKlu() => EndChan.GetEndKlu();
    public KLineUnit GetBeginKlu() => StartChan.GetBeginKlu();
    public int GetKluCnt() => GetEndKlu().Idx - GetBeginKlu().Idx + 1;

    public double CalMacdMetric(MACD_ALGO macdAlgo, bool isReverse)
    {
        return macdAlgo switch
        {
            MACD_ALGO.SLOPE => CalMacdSlope(),
            MACD_ALGO.AMP => CalMacdAmp(),
            _ => throw new ChanException($"unsupport macd_algo={macdAlgo} of Seg", ErrCode.PARA_ERROR)
        };
    }

    private float CalMacdSlope()
    {
        var beginKlu = GetBeginKlu();
        var endKlu = GetEndKlu();
        if (IsUp())
            return (endKlu.High - beginKlu.Low) / endKlu.High / (endKlu.Idx - beginKlu.Idx + 1);
        else
            return (beginKlu.High - endKlu.Low) / beginKlu.High / (endKlu.Idx - beginKlu.Idx + 1);
    }

    private float CalMacdAmp()
    {
        var beginKlu = GetBeginKlu();
        var endKlu = GetEndKlu();
        if (IsDown())
            return (beginKlu.High - endKlu.Low) / beginKlu.High;
        else
            return (endKlu.High - beginKlu.Low) / beginKlu.Low;
    }

    public void UpdateBiList(List<IChanLine> biLst, int idx1, int idx2)
    {
        for (int biIdx = idx1; biIdx <= idx2; biIdx++)
        {
            biLst[biIdx].ParentSeg = this;
            BiList.Add(biLst[biIdx]);
        }
        if (BiList.Count >= 3)
        {
            SupportTrendLine = new TrendLine(BiList, TREND_LINE_SIDE.INSIDE);
            ResistanceTrendLine = new TrendLine(BiList, TREND_LINE_SIDE.OUTSIDE);
        }
    }

    public Pivot? GetFirstMultiBiZs() => ZsLst.FirstOrDefault(zs => !zs.IsOneBiZs());
    public List<Pivot> GetMultiBiZsLst() => ZsLst.Where(zs => !zs.IsOneBiZs()).ToList();
    public Pivot? GetFinalMultiBiZs()
    {
        for (int zsIdx = ZsLst.Count - 1; zsIdx >= 0; zsIdx--)
        {
            if (!ZsLst[zsIdx].IsOneBiZs())
                return ZsLst[zsIdx];
        }
        return null;
    }

    public int GetMultiBiZsCnt() => ZsLst.Count(zs => !zs.IsOneBiZs());

    DateTime ICombineSource.TimeBegin => StartChan.BeginKlc.TimeBegin;
    DateTime ICombineSource.TimeEnd => EndChan.EndKlc.TimeBegin;
    float ICombineSource.CombineHigh => High();
    float ICombineSource.CombineLow => Low();
}
