namespace Stock.Service.Interface;

/// <summary>
/// 蜡烛图形态信号方向
/// </summary>
public enum PatternDirection
{
    /// <summary>中性/无明确方向</summary>
    Neutral,
    /// <summary>看涨/买入信号</summary>
    Bullish,
    /// <summary>看跌/卖出信号</summary>
    Bearish,
}

/// <summary>
/// 单个蜡烛图形态详情
/// </summary>
public class CandlestickPatternInfo
{
    /// <summary>形态枚举值</summary>
    public CandlestickPattern Pattern { get; set; }
    /// <summary>中文名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>形态描述</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>信号方向</summary>
    public PatternDirection Direction { get; set; }
    /// <summary>是否为反转形态</summary>
    public bool IsReversal { get; set; }
    /// <summary>信号强度 (1-5)</summary>
    public int Strength { get; set; }
}

/// <summary>
/// 日本蜡烛图形态
/// </summary>
[Flags]
public enum CandlestickPattern
{
    None = 0,

    // 单根K线形态
    Hammer = 1 << 0,
    HangingMan = 1 << 1,
    ShootingStar = 1 << 2,
    InvertedHammer = 1 << 3,
    Doji = 1 << 4,
    LongLeggedDoji = 1 << 5,
    DragonflyDoji = 1 << 6,
    GravestoneDoji = 1 << 7,
    SpinningTop = 1 << 8,
    Marubozu = 1 << 9,

    // 两根K线形态
    BullishEngulfing = 1 << 10,
    BearishEngulfing = 1 << 11,
    DarkCloudCover = 1 << 12,
    PiercingPattern = 1 << 13,
    BullishHarami = 1 << 14,
    BearishHarami = 1 << 15,
    HaramiCross = 1 << 16,

    // 三根K线形态
    MorningStar = 1 << 17,
    EveningStar = 1 << 18,
    ThreeWhiteSoldiers = 1 << 19,
    ThreeBlackCrows = 1 << 20,
}

public static class CandlestickPatternExtensions
{
    private static readonly Dictionary<CandlestickPattern, CandlestickPatternInfo> PatternDetails = new()
    {
        // 单根K线形态
        [CandlestickPattern.Hammer] = new()
        {
            Pattern = CandlestickPattern.Hammer,
            Name = "锤子线",
            Description = "出现在下跌趋势中，实体较小位于K线上端，下影线长度通常为实体的2倍以上，上影线极短或没有。表明空方力量衰竭，多方开始反攻。",
            Direction = PatternDirection.Bullish,
            IsReversal = true,
            Strength = 3,
        },
        [CandlestickPattern.HangingMan] = new()
        {
            Pattern = CandlestickPattern.HangingMan,
            Name = "上吊线",
            Description = "出现在上涨趋势中，形态与锤子线相同（小实体在上端，长下影线），但出现在高位。表明多方力量减弱，可能见顶回落。",
            Direction = PatternDirection.Bearish,
            IsReversal = true,
            Strength = 3,
        },
        [CandlestickPattern.ShootingStar] = new()
        {
            Pattern = CandlestickPattern.ShootingStar,
            Name = "流星线",
            Description = "出现在上涨趋势中，实体较小位于K线下端，上影线长度通常为实体的2倍以上，下影线极短。形似流星划过天空，表明上方抛压沉重。",
            Direction = PatternDirection.Bearish,
            IsReversal = true,
            Strength = 3,
        },
        [CandlestickPattern.InvertedHammer] = new()
        {
            Pattern = CandlestickPattern.InvertedHammer,
            Name = "倒锤子线",
            Description = "出现在下跌趋势中，形态与流星线相同（小实体在下端，长上影线），但出现在低位。表明多方试探性上攻，若次日确认则反转信号更强。",
            Direction = PatternDirection.Bullish,
            IsReversal = true,
            Strength = 2,
        },
        [CandlestickPattern.Doji] = new()
        {
            Pattern = CandlestickPattern.Doji,
            Name = "十字星",
            Description = "开盘价与收盘价几乎相等，形成十字形。表明多空双方力量均衡，市场处于犹豫状态，常出现在趋势转折点。",
            Direction = PatternDirection.Neutral,
            IsReversal = true,
            Strength = 2,
        },
        [CandlestickPattern.LongLeggedDoji] = new()
        {
            Pattern = CandlestickPattern.LongLeggedDoji,
            Name = "长腿十字星",
            Description = "十字星的一种，上下影线都很长。表明多空双方激烈博弈但最终势均力敌，出现在趋势末端时反转信号强烈。",
            Direction = PatternDirection.Neutral,
            IsReversal = true,
            Strength = 3,
        },
        [CandlestickPattern.DragonflyDoji] = new()
        {
            Pattern = CandlestickPattern.DragonflyDoji,
            Name = "蜻蜓十字星",
            Description = "十字星的一种，下影线很长而上影线很短或没有，形似蜻蜓。出现在下跌趋势末端时为强烈看涨信号。",
            Direction = PatternDirection.Bullish,
            IsReversal = true,
            Strength = 4,
        },
        [CandlestickPattern.GravestoneDoji] = new()
        {
            Pattern = CandlestickPattern.GravestoneDoji,
            Name = "墓碑十字星",
            Description = "十字星的一种，上影线很长而下影线很短或没有，形似墓碑。出现在上涨趋势末端时为强烈看跌信号。",
            Direction = PatternDirection.Bearish,
            IsReversal = true,
            Strength = 4,
        },
        [CandlestickPattern.SpinningTop] = new()
        {
            Pattern = CandlestickPattern.SpinningTop,
            Name = "纺锤线",
            Description = "实体很小，但有明显的上下影线。表明多空双方都在积极交易但未能取得决定性优势，常预示趋势可能发生变化。",
            Direction = PatternDirection.Neutral,
            IsReversal = false,
            Strength = 1,
        },
        [CandlestickPattern.Marubozu] = new()
        {
            Pattern = CandlestickPattern.Marubozu,
            Name = "光头光脚",
            Description = "几乎没有上下影线，实体很长。阳线表示多方完全控制市场，力量强劲；阴线表示空方完全主导。表明当前趋势力量强大。",
            Direction = PatternDirection.Neutral,
            IsReversal = false,
            Strength = 2,
        },

        // 两根K线形态
        [CandlestickPattern.BullishEngulfing] = new()
        {
            Pattern = CandlestickPattern.BullishEngulfing,
            Name = "看涨吞没",
            Description = "出现在下跌趋势中，第一根为阴线，第二根阳线实体完全包住第一根阴线实体。表明多方力量突然增强，压倒空方，是强烈的底部反转信号。",
            Direction = PatternDirection.Bullish,
            IsReversal = true,
            Strength = 4,
        },
        [CandlestickPattern.BearishEngulfing] = new()
        {
            Pattern = CandlestickPattern.BearishEngulfing,
            Name = "看跌吞没",
            Description = "出现在上涨趋势中，第一根为阳线，第二根阴线实体完全包住第一根阳线实体。表明空方力量突然爆发，压倒多方，是强烈的顶部反转信号。",
            Direction = PatternDirection.Bearish,
            IsReversal = true,
            Strength = 4,
        },
        [CandlestickPattern.DarkCloudCover] = new()
        {
            Pattern = CandlestickPattern.DarkCloudCover,
            Name = "乌云盖顶",
            Description = "出现在上涨趋势中，第一根为大阳线，第二根阴线开盘价高于前高，但收盘价深入前阳线实体50%以上。如同乌云遮住太阳，表明顶部可能形成。",
            Direction = PatternDirection.Bearish,
            IsReversal = true,
            Strength = 4,
        },
        [CandlestickPattern.PiercingPattern] = new()
        {
            Pattern = CandlestickPattern.PiercingPattern,
            Name = "刺透形态",
            Description = "出现在下跌趋势中，第一根为大阴线，第二根阳线开盘价低于前低，但收盘价深入前阴线实体50%以上。表明多方开始反攻，是底部反转信号。",
            Direction = PatternDirection.Bullish,
            IsReversal = true,
            Strength = 4,
        },
        [CandlestickPattern.BullishHarami] = new()
        {
            Pattern = CandlestickPattern.BullishHarami,
            Name = "看涨孕线",
            Description = "出现在下跌趋势中，第一根为大阴线，第二根小阳线实体完全包含在第一根实体内。如同母体中的胎儿，表明下跌动能衰竭，可能反转向上。",
            Direction = PatternDirection.Bullish,
            IsReversal = true,
            Strength = 3,
        },
        [CandlestickPattern.BearishHarami] = new()
        {
            Pattern = CandlestickPattern.BearishHarami,
            Name = "看跌孕线",
            Description = "出现在上涨趋势中，第一根为大阳线，第二根小阴线实体完全包含在第一根实体内。表明上涨动能减弱，可能反转向下。",
            Direction = PatternDirection.Bearish,
            IsReversal = true,
            Strength = 3,
        },
        [CandlestickPattern.HaramiCross] = new()
        {
            Pattern = CandlestickPattern.HaramiCross,
            Name = "十字孕线",
            Description = "孕线形态中第二根为十字星。比普通孕线更强烈的反转信号，表明市场由犹豫转向新的方向。",
            Direction = PatternDirection.Neutral,
            IsReversal = true,
            Strength = 4,
        },

        // 三根K线形态
        [CandlestickPattern.MorningStar] = new()
        {
            Pattern = CandlestickPattern.MorningStar,
            Name = "早晨之星",
            Description = "出现在下跌趋势中，第一根为大阴线，第二根小实体（十字星最佳），第三根阳线收盘深入第一根实体。如同黎明前的黑暗后出现曙光，是强烈的底部反转信号。",
            Direction = PatternDirection.Bullish,
            IsReversal = true,
            Strength = 5,
        },
        [CandlestickPattern.EveningStar] = new()
        {
            Pattern = CandlestickPattern.EveningStar,
            Name = "黄昏之星",
            Description = "出现在上涨趋势中，第一根为大阳线，第二根小实体（十字星最佳），第三根阴线收盘深入第一根实体。如同黄昏降临，是强烈的顶部反转信号。",
            Direction = PatternDirection.Bearish,
            IsReversal = true,
            Strength = 5,
        },
        [CandlestickPattern.ThreeWhiteSoldiers] = new()
        {
            Pattern = CandlestickPattern.ThreeWhiteSoldiers,
            Name = "白三兵",
            Description = "出现在下跌趋势或盘整后，连续三根阳线，收盘价依次抬高，每根开盘价在前一根实体内。表明多方力量稳步增强，是强烈的看涨持续/反转信号。",
            Direction = PatternDirection.Bullish,
            IsReversal = false,
            Strength = 4,
        },
        [CandlestickPattern.ThreeBlackCrows] = new()
        {
            Pattern = CandlestickPattern.ThreeBlackCrows,
            Name = "三只乌鸦",
            Description = "出现在上涨趋势或盘整后，连续三根阴线，收盘价依次降低，每根开盘价在前一根实体内。表明空方力量稳步增强，是强烈的看跌持续/反转信号。",
            Direction = PatternDirection.Bearish,
            IsReversal = false,
            Strength = 4,
        },
    };

    /// <summary>
    /// 获取指定形态的详细信息
    /// </summary>
    public static CandlestickPatternInfo? GetDetail(this CandlestickPattern pattern)
    {
        if (pattern == CandlestickPattern.None) return null;
        return PatternDetails.TryGetValue(pattern, out var detail) ? detail : null;
    }

    /// <summary>
    /// 将形态枚举展开为详细信息列表
    /// </summary>
    public static List<CandlestickPatternInfo> ToDetails(this CandlestickPattern pattern)
    {
        var result = new List<CandlestickPatternInfo>();
        if (pattern == CandlestickPattern.None) return result;

        foreach (CandlestickPattern value in Enum.GetValues<CandlestickPattern>())
        {
            if (value != CandlestickPattern.None && pattern.HasFlag(value))
            {
                var detail = value.GetDetail();
                if (detail != null) result.Add(detail);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取形态的中文名称列表
    /// </summary>
    public static List<string> ToNames(this CandlestickPattern pattern)
    {
        return pattern.ToDetails().Select(d => d.Name).ToList();
    }

    /// <summary>
    /// 判断整体信号方向（多个形态同时出现时，按强度加权判断）
    /// </summary>
    public static PatternDirection OverallDirection(this CandlestickPattern pattern)
    {
        var details = pattern.ToDetails();
        if (details.Count == 0) return PatternDirection.Neutral;

        int bullishScore = details.Where(d => d.Direction == PatternDirection.Bullish).Sum(d => d.Strength);
        int bearishScore = details.Where(d => d.Direction == PatternDirection.Bearish).Sum(d => d.Strength);

        return bullishScore > bearishScore ? PatternDirection.Bullish :
               bearishScore > bullishScore ? PatternDirection.Bearish :
               PatternDirection.Neutral;
    }

    /// <summary>
    /// 获取整体信号描述
    /// </summary>
    public static string OverallSignal(this CandlestickPattern pattern)
    {
        var direction = pattern.OverallDirection();
        var details = pattern.ToDetails();

        if (details.Count == 0) return "无明显形态信号";

        var names = string.Join(" + ", details.Select(d => d.Name));
        var directionText = direction switch
        {
            PatternDirection.Bullish => "看涨",
            PatternDirection.Bearish => "看跌",
            _ => "中性/观望",
        };

        return $"[{directionText}] {names}";
    }
}
