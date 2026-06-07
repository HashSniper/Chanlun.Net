namespace Stock.Service.Interface;

/// <summary>
/// K线指标计算服务接口
/// </summary>
public interface IKLineIndicatorService
{
    /// <summary>
    /// 获取指定股票在指定时间范围内的K线数据及其技术指标
    /// </summary>
    Task<KLineIndicatorResult> GetKlinesWithIndicatorsAsync(GetKLineQuery query, CancellationToken ct = default);
}
