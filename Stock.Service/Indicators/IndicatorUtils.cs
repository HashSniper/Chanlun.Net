namespace Stock.Service.Indicators;

/// <summary>
/// 指标计算共享工具方法
/// </summary>
internal static class IndicatorUtils
{
    /// <summary>double? 转 decimal?</summary>
    public static decimal? ToDecimal(double? value)
    {
        return value.HasValue ? (decimal)value.Value : null;
    }

    /// <summary>KDJ 的 J 值 = 3K - 2D</summary>
    public static decimal? CalculateJ(double? k, double? d)
    {
        if (!k.HasValue || !d.HasValue) return null;
        return (decimal)(3 * k.Value - 2 * d.Value);
    }
}
