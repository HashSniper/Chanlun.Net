using Chan.Lib;
using Chan.Lib.Common;
using Chan.Lib.Combiner;
using Chan.Lib.KLines;
using Chan.Lib.Bis;
using Chan.Lib.ZS;
using Chan.Lib.BuySellPoints;
using Chan.Lib.Indicators;

namespace Chan.Lib.Seg;

public class Segment : IBiLine
{
    public int Idx { get; }
    public Bi StartBi { get; }
    public Bi EndBi { get; private set; }
    public bool IsSure { get; set; }
    public BI_DIR Dir { get; }
    public List<Pivot> ZsLst { get; } = new();
    public EigenFeature? EigenFx { get; set; }
    public int? SegIdx { get; set; }
    public Segment? ParentSeg { get; set; }
    public Segment? Pre { get; set; }
    public Segment? Next { get; set; }
    IBiLine? IBiLine.Pre => Pre;
    IBiLine? IBiLine.Next => Next;
    public BuySellPoint? Bsp { get; set; }
    public List<Bi> BiList { get; } = new();
    public string Reason { get; }
    public TrendLine? SupportTrendLine { get; set; }
    public TrendLine? ResistanceTrendLine { get; set; }
    public bool EleInsideIsSure { get; set; } = false;

    public Segment(int idx, Bi startBi, Bi endBi, bool isSure = true, BI_DIR? segDir = null, string reason = "normal")
    {
        Idx = idx;
        StartBi = startBi;
        EndBi = endBi;
        IsSure = isSure;
        Dir = segDir ?? endBi.Dir;
        Reason = reason;
        if (endBi.Idx - startBi.Idx < 2)
            IsSure = false;
        Check();
    }

    public void SetSegIdx(int idx) => SegIdx = idx;

    public void Check()
    {
        if (!IsSure) return;
        if (IsDown())
        {
            if (StartBi.GetBeginVal() < EndBi.GetEndVal())
                throw new ChanException($"下降线段起始点应该高于结束点! idx={Idx}", ErrCode.SEG_END_VALUE_ERR);
        }
        else if (StartBi.GetBeginVal() > EndBi.GetEndVal())
        {
            throw new ChanException($"上升线段起始点应该低于结束点! idx={Idx}", ErrCode.SEG_END_VALUE_ERR);
        }
        if (EndBi.Idx - StartBi.Idx < 2)
            throw new ChanException($"线段({StartBi.Idx}-{EndBi.Idx})长度不能小于2! idx={Idx}", ErrCode.SEG_LEN_ERR);
    }

    public override string ToString() => $"{StartBi.Idx}->{EndBi.Idx}: {Dir} {IsSure}";

    public void AddZs(Pivot zs) => ZsLst.Insert(0, zs);
    public void ClearZsLst() => ZsLst.Clear();

    public float CalKluSlope() => (GetEndVal() - GetBeginVal()) / (GetEndKlu().Idx - GetBeginKlu().Idx) / GetBeginVal();
    public float CalAmp() => (GetEndVal() - GetBeginVal()) / GetBeginVal();
    public int CalBiCnt() => EndBi.Idx - StartBi.Idx + 1;

    public float Low() => IsDown() ? EndBi.GetEndKlu().Low : StartBi.GetBeginKlu().Low;
    public float High() => IsUp() ? EndBi.GetEndKlu().High : StartBi.GetBeginKlu().High;
    public bool IsDown() => Dir == BI_DIR.DOWN;
    public bool IsUp() => Dir == BI_DIR.UP;
    public float GetEndVal() => EndBi.GetEndVal();
    public float GetBeginVal() => StartBi.GetBeginVal();
    public float Amp() => Math.Abs(GetEndVal() - GetBeginVal());
    public KLineUnit GetEndKlu() => EndBi.GetEndKlu();
    public KLineUnit GetBeginKlu() => StartBi.GetBeginKlu();
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

    public void UpdateBiList(List<Bi> biLst, int idx1, int idx2)
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

    object ICombineSource.TimeBegin => StartBi.BeginKlc.Idx;
    object ICombineSource.TimeEnd => EndBi.EndKlc.Idx;
    float ICombineSource.CombineHigh => High();
    float ICombineSource.CombineLow => Low();
}
