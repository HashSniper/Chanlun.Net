using Stock.Data.Entities;
using Stock.Service.Interface;

namespace Stock.Service.Indicators;

/// <summary>
/// 蜡烛图形态指标处理器 —— 识别单根/两根/三根K线形态，推导方向和信号
/// </summary>
public class CandlestickPatternIndicatorProcessor : IIndicatorProcessor
{
    public string Name => "CandlestickPattern";

    public Task ProcessAsync(List<KlineBase> klines, List<KLineIndicatorItem> items, CancellationToken ct = default)
    {
        for (int i = 0; i < klines.Count; i++)
        {
            var patterns = Recognize(klines, i);
            items[i].Candlestick.Patterns = patterns;
            items[i].Candlestick.PatternDetails = patterns.ToDetails();
            items[i].Candlestick.PatternDirection = patterns.OverallDirection();
            items[i].Candlestick.PatternSignal = patterns.OverallSignal();
        }

        return Task.CompletedTask;
    }

    #region 辅助方法

    private decimal Body(decimal open, decimal close) => Math.Abs(close - open);
    private decimal UpperShadow(decimal open, decimal high, decimal close) => high - Math.Max(open, close);
    private decimal LowerShadow(decimal open, decimal low, decimal close) => Math.Min(open, close) - low;
    private bool IsBullish(decimal open, decimal close) => close > open;
    private bool IsBearish(decimal open, decimal close) => close < open;
    private bool IsUpTrend(KlineBase? prev, KlineBase? prevPrev)
    {
        if (prev == null) return false;
        if (prevPrev == null) return prev.Close >= prev.Open;
        return prev.Close > prevPrev.Close;
    }
    private bool IsDownTrend(KlineBase? prev, KlineBase? prevPrev)
    {
        if (prev == null) return false;
        if (prevPrev == null) return prev.Close < prev.Open;
        return prev.Close < prevPrev.Close;
    }

    #endregion

    #region 单根K线形态

    /// <summary>十字星：开盘价与收盘价几乎相等</summary>
    private bool IsDoji(KlineBase k)
    {
        var range = k.High - k.Low;
        if (range == 0) return true;
        var bodyRatio = Math.Abs(k.Close - k.Open) / range;
        return bodyRatio < 0.05m;
    }

    /// <summary>长腿十字星：十字星且上下影线都很长</summary>
    private bool IsLongLeggedDoji(KlineBase k)
    {
        if (!IsDoji(k)) return false;
        var range = k.High - k.Low;
        var upper = UpperShadow(k.Open, k.High, k.Close);
        var lower = LowerShadow(k.Open, k.Low, k.Close);
        return upper > range * 0.3m && lower > range * 0.3m;
    }

    /// <summary>蜻蜓十字星：十字星，下影线很长，上影线很短</summary>
    private bool IsDragonflyDoji(KlineBase k)
    {
        if (!IsDoji(k)) return false;
        var range = k.High - k.Low;
        if (range == 0) return false;
        var upper = UpperShadow(k.Open, k.High, k.Close);
        var lower = LowerShadow(k.Open, k.Low, k.Close);
        return lower > range * 0.6m && upper < range * 0.1m;
    }

    /// <summary>墓碑十字星：十字星，上影线很长，下影线很短</summary>
    private bool IsGravestoneDoji(KlineBase k)
    {
        if (!IsDoji(k)) return false;
        var range = k.High - k.Low;
        if (range == 0) return false;
        var upper = UpperShadow(k.Open, k.High, k.Close);
        var lower = LowerShadow(k.Open, k.Low, k.Close);
        return upper > range * 0.6m && lower < range * 0.1m;
    }

    /// <summary>纺锤线：实体很小，但有明显的上下影线</summary>
    private bool IsSpinningTop(KlineBase k)
    {
        var body = Body(k.Open, k.Close);
        var range = k.High - k.Low;
        if (range == 0) return false;
        var bodyRatio = body / range;
        var upper = UpperShadow(k.Open, k.High, k.Close);
        var lower = LowerShadow(k.Open, k.Low, k.Close);
        return bodyRatio < 0.15m && upper > body * 0.5m && lower > body * 0.5m;
    }

    /// <summary>锤子线：下跌趋势中，下影线 > 实体2倍，上影线很短，实体在顶部</summary>
    private bool IsHammer(KlineBase k, KlineBase? prev, KlineBase? prevPrev)
    {
        if (!IsDownTrend(prev, prevPrev)) return false;
        var body = Body(k.Open, k.Close);
        var lower = LowerShadow(k.Open, k.Low, k.Close);
        var upper = UpperShadow(k.Open, k.High, k.Close);
        if (body == 0) return false;
        return lower > body * 2 && upper < body * 0.5m && IsBullish(k.Open, k.Close);
    }

    /// <summary>上吊线：上涨趋势中，下影线 > 实体2倍，上影线很短，实体在顶部</summary>
    private bool IsHangingMan(KlineBase k, KlineBase? prev, KlineBase? prevPrev)
    {
        if (!IsUpTrend(prev, prevPrev)) return false;
        var body = Body(k.Open, k.Close);
        var lower = LowerShadow(k.Open, k.Low, k.Close);
        var upper = UpperShadow(k.Open, k.High, k.Close);
        if (body == 0) return false;
        return lower > body * 2 && upper < body * 0.5m;
    }

    /// <summary>流星线：上涨趋势中，上影线 > 实体2倍，下影线很短，实体在底部</summary>
    private bool IsShootingStar(KlineBase k, KlineBase? prev, KlineBase? prevPrev)
    {
        if (!IsUpTrend(prev, prevPrev)) return false;
        var body = Body(k.Open, k.Close);
        var upper = UpperShadow(k.Open, k.High, k.Close);
        var lower = LowerShadow(k.Open, k.Low, k.Close);
        if (body == 0) return false;
        return upper > body * 2 && lower < body * 0.5m && IsBearish(k.Open, k.Close);
    }

    /// <summary>倒锤子线：下跌趋势中，上影线 > 实体2倍，下影线很短，实体在底部</summary>
    private bool IsInvertedHammer(KlineBase k, KlineBase? prev, KlineBase? prevPrev)
    {
        if (!IsDownTrend(prev, prevPrev)) return false;
        var body = Body(k.Open, k.Close);
        var upper = UpperShadow(k.Open, k.High, k.Close);
        var lower = LowerShadow(k.Open, k.Low, k.Close);
        if (body == 0) return false;
        return upper > body * 2 && lower < body * 0.5m && IsBullish(k.Open, k.Close);
    }

    /// <summary>光头光脚：几乎没有上下影线</summary>
    private bool IsMarubozu(KlineBase k)
    {
        var body = Body(k.Open, k.Close);
        var range = k.High - k.Low;
        if (range == 0) return false;
        return body / range > 0.95m;
    }

    #endregion

    #region 两根K线形态

    /// <summary>看涨吞没：第一根阴线，第二根阳线，第二根实体完全包住第一根</summary>
    private bool IsBullishEngulfing(KlineBase prev, KlineBase curr)
    {
        if (!IsBearish(prev.Open, prev.Close)) return false;
        if (!IsBullish(curr.Open, curr.Close)) return false;
        return curr.Open < prev.Close && curr.Close > prev.Open;
    }

    /// <summary>看跌吞没：第一根阳线，第二根阴线，第二根实体完全包住第一根</summary>
    private bool IsBearishEngulfing(KlineBase prev, KlineBase curr)
    {
        if (!IsBullish(prev.Open, prev.Close)) return false;
        if (!IsBearish(curr.Open, curr.Close)) return false;
        return curr.Open > prev.Close && curr.Close < prev.Open;
    }

    /// <summary>乌云盖顶：上涨趋势，第一根阳线，第二根阴线开盘高于前高，收盘深入到前实体50%以下</summary>
    private bool IsDarkCloudCover(KlineBase prev, KlineBase curr, KlineBase? prevPrev)
    {
        if (!IsUpTrend(prev, prevPrev)) return false;
        if (!IsBullish(prev.Open, prev.Close)) return false;
        if (!IsBearish(curr.Open, curr.Close)) return false;
        if (curr.Open <= prev.Close) return false;
        var prevBody = prev.Close - prev.Open;
        var penetration = prev.Close - curr.Close;
        return penetration > prevBody * 0.5m;
    }

    /// <summary>刺透形态：下跌趋势，第一根阴线，第二根阳线开盘低于前低，收盘深入到前实体50%以上</summary>
    private bool IsPiercingPattern(KlineBase prev, KlineBase curr, KlineBase? prevPrev)
    {
        if (!IsDownTrend(prev, prevPrev)) return false;
        if (!IsBearish(prev.Open, prev.Close)) return false;
        if (!IsBullish(curr.Open, curr.Close)) return false;
        if (curr.Open >= prev.Close) return false;
        var prevBody = prev.Open - prev.Close;
        var penetration = curr.Close - prev.Close;
        return penetration > prevBody * 0.5m;
    }

    /// <summary>看涨孕线：第一根大阴线，第二根小阳线完全在第一根实体内</summary>
    private bool IsBullishHarami(KlineBase prev, KlineBase curr)
    {
        if (!IsBearish(prev.Open, prev.Close)) return false;
        if (!IsBullish(curr.Open, curr.Close)) return false;
        var prevBody = Body(prev.Open, prev.Close);
        var currBody = Body(curr.Open, curr.Close);
        return currBody < prevBody * 0.6m &&
               curr.Open > prev.Close && curr.Close < prev.Open;
    }

    /// <summary>看跌孕线：第一根大阳线，第二根小阴线完全在第一根实体内</summary>
    private bool IsBearishHarami(KlineBase prev, KlineBase curr)
    {
        if (!IsBullish(prev.Open, prev.Close)) return false;
        if (!IsBearish(curr.Open, curr.Close)) return false;
        var prevBody = Body(prev.Open, prev.Close);
        var currBody = Body(curr.Open, curr.Close);
        return currBody < prevBody * 0.6m &&
               curr.Close > prev.Open && curr.Open < prev.Close;
    }

    /// <summary>十字孕线：孕线中第二根是十字星</summary>
    private bool IsHaramiCross(KlineBase prev, KlineBase curr)
    {
        if (!IsDoji(curr)) return false;
        return (IsBullishHarami(prev, curr) || IsBearishHarami(prev, curr));
    }

    #endregion

    #region 三根K线形态

    /// <summary>早晨之星：下跌趋势，第一根大阴线，第二根小实体，第三根阳线收盘深入第一根实体</summary>
    private bool IsMorningStar(KlineBase first, KlineBase second, KlineBase third)
    {
        if (!IsBearish(first.Open, first.Close)) return false;
        if (!IsBullish(third.Open, third.Close)) return false;
        var firstBody = Body(first.Open, first.Close);
        var secondBody = Body(second.Open, second.Close);
        var thirdBody = Body(third.Open, third.Close);
        if (secondBody > firstBody * 0.5m) return false;
        if (thirdBody < firstBody * 0.5m) return false;
        return third.Close > (first.Open + first.Close) / 2;
    }

    /// <summary>黄昏之星：上涨趋势，第一根大阳线，第二根小实体，第三根阴线收盘深入第一根实体</summary>
    private bool IsEveningStar(KlineBase first, KlineBase second, KlineBase third)
    {
        if (!IsBullish(first.Open, first.Close)) return false;
        if (!IsBearish(third.Open, third.Close)) return false;
        var firstBody = Body(first.Open, first.Close);
        var secondBody = Body(second.Open, second.Close);
        var thirdBody = Body(third.Open, third.Close);
        if (secondBody > firstBody * 0.5m) return false;
        if (thirdBody < firstBody * 0.5m) return false;
        return third.Close < (first.Open + first.Close) / 2;
    }

    /// <summary>白三兵：连续三根阳线，收盘价越来越高，开盘价在前一根实体内</summary>
    private bool IsThreeWhiteSoldiers(KlineBase first, KlineBase second, KlineBase third)
    {
        if (!IsBullish(first.Open, first.Close)) return false;
        if (!IsBullish(second.Open, second.Close)) return false;
        if (!IsBullish(third.Open, third.Close)) return false;
        if (second.Close <= first.Close) return false;
        if (third.Close <= second.Close) return false;
        return second.Open > first.Open && second.Open < first.Close &&
               third.Open > second.Open && third.Open < second.Close;
    }

    /// <summary>三只乌鸦：连续三根阴线，收盘价越来越低，开盘价在前一根实体内</summary>
    private bool IsThreeBlackCrows(KlineBase first, KlineBase second, KlineBase third)
    {
        if (!IsBearish(first.Open, first.Close)) return false;
        if (!IsBearish(second.Open, second.Close)) return false;
        if (!IsBearish(third.Open, third.Close)) return false;
        if (second.Close >= first.Close) return false;
        if (third.Close >= second.Close) return false;
        return second.Open < first.Open && second.Open > first.Close &&
               third.Open < second.Open && third.Open > second.Close;
    }

    #endregion

    /// <summary>
    /// 识别指定位置的K线形态
    /// </summary>
    private CandlestickPattern Recognize(List<KlineBase> klines, int index)
    {
        var pattern = CandlestickPattern.None;
        var curr = klines[index];
        KlineBase? prev = index > 0 ? klines[index - 1] : null;
        KlineBase? prevPrev = index > 1 ? klines[index - 2] : null;

        // 单根K线形态
        if (IsDoji(curr)) pattern |= CandlestickPattern.Doji;
        if (IsLongLeggedDoji(curr)) pattern |= CandlestickPattern.LongLeggedDoji;
        if (IsDragonflyDoji(curr)) pattern |= CandlestickPattern.DragonflyDoji;
        if (IsGravestoneDoji(curr)) pattern |= CandlestickPattern.GravestoneDoji;
        if (IsSpinningTop(curr)) pattern |= CandlestickPattern.SpinningTop;
        if (IsMarubozu(curr)) pattern |= CandlestickPattern.Marubozu;
        if (IsHammer(curr, prev, prevPrev)) pattern |= CandlestickPattern.Hammer;
        if (IsHangingMan(curr, prev, prevPrev)) pattern |= CandlestickPattern.HangingMan;
        if (IsShootingStar(curr, prev, prevPrev)) pattern |= CandlestickPattern.ShootingStar;
        if (IsInvertedHammer(curr, prev, prevPrev)) pattern |= CandlestickPattern.InvertedHammer;

        // 两根K线形态
        if (prev != null)
        {
            if (IsBullishEngulfing(prev, curr)) pattern |= CandlestickPattern.BullishEngulfing;
            if (IsBearishEngulfing(prev, curr)) pattern |= CandlestickPattern.BearishEngulfing;
            if (IsDarkCloudCover(prev, curr, prevPrev)) pattern |= CandlestickPattern.DarkCloudCover;
            if (IsPiercingPattern(prev, curr, prevPrev)) pattern |= CandlestickPattern.PiercingPattern;
            if (IsBullishHarami(prev, curr)) pattern |= CandlestickPattern.BullishHarami;
            if (IsBearishHarami(prev, curr)) pattern |= CandlestickPattern.BearishHarami;
            if (IsHaramiCross(prev, curr)) pattern |= CandlestickPattern.HaramiCross;
        }

        // 三根K线形态
        if (prev != null && prevPrev != null)
        {
            if (IsMorningStar(prevPrev, prev, curr)) pattern |= CandlestickPattern.MorningStar;
            if (IsEveningStar(prevPrev, prev, curr)) pattern |= CandlestickPattern.EveningStar;
            if (IsThreeWhiteSoldiers(prevPrev, prev, curr)) pattern |= CandlestickPattern.ThreeWhiteSoldiers;
            if (IsThreeBlackCrows(prevPrev, prev, curr)) pattern |= CandlestickPattern.ThreeBlackCrows;
        }

        return pattern;
    }
}
