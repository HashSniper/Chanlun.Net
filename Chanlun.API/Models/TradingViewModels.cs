namespace Chanlun.API.Models;

public class TvKlineBar
{
    public DateTime Time { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }

    // 缠论买卖点
    public bool? IsBuy1 { get; set; }
    public bool? IsBuy2 { get; set; }
    public bool? IsBuy3 { get; set; }
    public bool? IsSell1 { get; set; }
    public bool? IsSell2 { get; set; }
    public bool? IsSell3 { get; set; }

    // MA 移动平均线
    public decimal? MA5 { get; set; }
    public decimal? MA10 { get; set; }
    public decimal? MA20 { get; set; }
    public decimal? MA60 { get; set; }

    // MACD
    public decimal? MacdDif { get; set; }
    public decimal? MacdDea { get; set; }
    public decimal? MacdHistogram { get; set; }

    // KDJ
    public decimal? KdjK { get; set; }
    public decimal? KdjD { get; set; }
    public decimal? KdjJ { get; set; }
    public string KdjSignal { get; set; } = string.Empty;
    public bool? KdjBullish { get; set; }
    public bool KdjGoldenCross { get; set; }
    public bool KdjDeathCross { get; set; }
    public bool KdjBottomDivergence { get; set; }
    public bool KdjTopDivergence { get; set; }

    // RSI
    public decimal? Rsi6 { get; set; }
    public decimal? Rsi12 { get; set; }
    public decimal? Rsi24 { get; set; }

    // BOLL 布林带
    public decimal? BollUpper { get; set; }
    public decimal? BollMiddle { get; set; }
    public decimal? BollLower { get; set; }

    // 日本蜡烛图形态
    public List<string> Patterns { get; set; } = [];
    public string PatternDirection { get; set; } = string.Empty;
    public string PatternSignal { get; set; } = string.Empty;

    // 量能关系指标
    public decimal? VolumeRatio5 { get; set; }
    public decimal? VolumeChangePct { get; set; }
    public decimal? Obv { get; set; }
    public string VolumeSignal { get; set; } = string.Empty;
    public bool? VolumeBullish { get; set; }

    // 海龟交易法则指标
    public decimal? TurtleHigh20 { get; set; }
    public decimal? TurtleHigh50 { get; set; }
    public decimal? TurtleLow20 { get; set; }
    public decimal? TurtleLow50 { get; set; }
    public bool TurtleBreakoutHigh20 { get; set; }
    public bool TurtleBreakoutHigh50 { get; set; }
    public bool TurtleBreakdownLow20 { get; set; }
    public bool TurtleBreakdownLow50 { get; set; }
    public string TurtleSignal { get; set; } = string.Empty;
    public bool? TurtleBullish { get; set; }
}

public class TvChanlunRequest
{
    public string Symbol { get; set; } = string.Empty;
    public List<TvKlineBar> Bars { get; set; } = [];
}

public class TvBiItem
{
    public long StartTime { get; set; }
    public decimal StartPrice { get; set; }
    public long EndTime { get; set; }
    public decimal EndPrice { get; set; }
    public string Direction { get; set; } = string.Empty;
}

public class TvSegItem
{
    public long StartTime { get; set; }
    public decimal StartPrice { get; set; }
    public long EndTime { get; set; }
    public decimal EndPrice { get; set; }
    public string Direction { get; set; } = string.Empty;
}

public class TvPivotItem
{
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public decimal ZG { get; set; }
    public decimal ZD { get; set; }
    public decimal GG { get; set; }
    public decimal DD { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
}

public class TvMergedKLine
{
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public string Direction { get; set; } = string.Empty;
}

public class TvChanlunResponse
{
    public string Symbol { get; set; } = string.Empty;
    public int BarCount { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public DateTime FromTime { get; set; }
    public DateTime ToTime { get; set; }

    public List<TvKlineBar> Bars { get; set; } = [];
    public List<TvBiItem> BiList { get; set; } = [];
    public List<TvSegItem> SegList { get; set; } = [];
    public List<TvPivotItem> BiPivotList { get; set; } = [];
    public List<TvPivotItem> SegPivotList { get; set; } = [];
    public List<TvMergedKLine> MergedKLines { get; set; } = [];
}

public class TvUdfConfig
{
    public bool Supports_search { get; set; } = true;
    public bool Supports_group_request { get; set; } = false;
    public bool Supports_marks { get; set; } = false;
    public bool Supports_timescale_marks { get; set; } = false;
    public bool Supports_time { get; set; } = true;
    public List<string> Supported_resolutions { get; set; } = ["1", "5", "15", "30", "60", "240", "D", "W", "M"];
    public List<string> Exchanges { get; set; } = ["SH", "SZ", "BJ"];
    public List<string> Symbols_types { get; set; } = ["stock", "index", "etf"];
}

public class TvUdfSymbolInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "stock";
    public string Session { get; set; } = "0900-1130,1300-1500";
    public string Exchange { get; set; } = "SH";
    public string Listed_exchange { get; set; } = "SH";
    public string Timezone { get; set; } = "Asia/Shanghai";
    public string Pricescale { get; set; } = "100";
    public int Minmov { get; set; } = 1;
    public int Minmove2 { get; set; } = 0;
    public bool Has_intraday { get; set; } = true;
    public List<string> Supported_resolutions { get; set; } = ["1", "5", "15", "30", "60", "240", "D", "W", "M"];
    public bool Has_daily { get; set; } = true;
    public bool Has_weekly_and_monthly { get; set; } = true;
    public bool Has_empty_bars { get; set; } = false;
    public bool Force_session_rebuild { get; set; } = true;
}

public class TvUdfHistory
{
    public string S { get; set; } = "ok";
    public string? Errmsg { get; set; }
    public long[]? T { get; set; }
    public decimal[]? O { get; set; }
    public decimal[]? H { get; set; }
    public decimal[]? L { get; set; }
    public decimal[]? C { get; set; }
    public decimal[]? V { get; set; }
}

public class TvUdfSearchResultItem
{
    public string Symbol { get; set; } = string.Empty;
    public string Full_name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
