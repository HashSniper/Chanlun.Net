namespace Chan.Lib.Common;

public enum DATA_SRC
{
    BAO_STOCK,
    CcxtApi,
    CSV,
    AKSHARE
}

public enum KL_TYPE
{
    K_1M,
    K_3M,
    K_5M,
    K_15M,
    K_30M,
    K_60M,
    K_DAY,
    K_WEEK,
    K_MON,
    K_QUARTER,
    K_YEAR
}

public enum KLINE_DIR
{
    UP,
    DOWN,
    COMBINE,
    INCLUDED
}

public enum FX_TYPE
{
    BOTTOM,
    TOP,
    UNKNOWN
}

public enum BI_DIR
{
    UP,
    DOWN
}

public enum BI_TYPE
{
    UNKNOWN,
    STRICT,
    SUB_VALUE,
    TIAOKONG_THRED,
    DAHENG,
    TUIBI,
    UNSTRICT,
    TIAOKONG_VALUE
}

public enum BSP_TYPE
{
    T1,
    T1P,
    T2,
    T2S,
    T3A,
    T3B
}

public static class BSPTypeExtensions
{
    public static char MainType(this BSP_TYPE type)
    {
        return type switch
        {
            BSP_TYPE.T1 => '1',
            BSP_TYPE.T1P => '1',
            BSP_TYPE.T2 => '2',
            BSP_TYPE.T2S => '2',
            BSP_TYPE.T3A => '3',
            BSP_TYPE.T3B => '3',
            _ => '0'
        };
    }
}

public enum AUTYPE
{
    QFQ,
    HFQ,
    NONE
}

public enum TREND_TYPE
{
    MEAN,
    MAX,
    MIN
}

public enum TREND_LINE_SIDE
{
    INSIDE,
    OUTSIDE
}

public enum LEFT_SEG_METHOD
{
    ALL,
    PEAK
}

public enum FX_CHECK_METHOD
{
    STRICT,
    LOSS,
    HALF,
    TOTALLY
}

public enum SEG_TYPE
{
    BI,
    SEG
}

public enum MACD_ALGO
{
    AREA,
    PEAK,
    FULL_AREA,
    DIFF,
    SLOPE,
    AMP,
    VOLUMN,
    AMOUNT,
    VOLUMN_AVG,
    AMOUNT_AVG,
    TURNRATE_AVG,
    RSI
}

public static class DATA_FIELD
{
    public const string FIELD_TIME = "time_key";
    public const string FIELD_OPEN = "open";
    public const string FIELD_HIGH = "high";
    public const string FIELD_LOW = "low";
    public const string FIELD_CLOSE = "close";
    public const string FIELD_VOLUME = "volume";
    public const string FIELD_TURNOVER = "turnover";
    public const string FIELD_TURNRATE = "turnover_rate";

    public static readonly string[] TRADE_INFO_LST = { FIELD_VOLUME, FIELD_TURNOVER, FIELD_TURNRATE };
}
