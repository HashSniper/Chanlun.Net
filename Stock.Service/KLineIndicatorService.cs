using Stock.Data.Entities;
using Stock.Data.Repositories;
using Stock.Service.Indicators;
using Stock.Service.Interface;

namespace Stock.Service;

/// <summary>
/// K线指标计算服务 —— 编排器模式
/// 
/// 职责：获取K线数据 → 初始化结果壳 → 逐个调用指标处理器 → 返回结果
/// 具体计算逻辑已分离到各 IIndicatorProcessor 实现类中
/// </summary>
public class KLineIndicatorService : IKLineIndicatorService
{
    private readonly IStockRepository _stockRepository;
    private readonly IEnumerable<IIndicatorProcessor> _processors;

    public KLineIndicatorService(
        IStockRepository stockRepository,
        IEnumerable<IIndicatorProcessor> processors)
    {
        _stockRepository = stockRepository;
        _processors = processors;
    }

    public async Task<KLineIndicatorResult> GetKlinesWithIndicatorsAsync(GetKLineQuery query, CancellationToken ct = default)
    {
        // 1. 从数据库获取K线数据
        var klines = (await _stockRepository.GetKlinesAsync<KlineBase>(
            query.Symbol,
            query.Resolution,
            query.FromTime,
            query.ToTime,
            ct)).ToList();

        var result = new KLineIndicatorResult
        {
            Symbol = query.Symbol,
            Resolution = query.Resolution,
            FromTime = query.FromTime,
            ToTime = query.ToTime,
            Count = klines.Count,
        };

        if (klines.Count == 0)
        {
            return result;
        }

        // 2. 初始化结果壳（按索引与 klines 对齐）
        result.Items = klines.Select(k => new KLineIndicatorItem { Kline = k }).ToList();

        // 3. 逐个异步调用指标处理器计算并填充结果
        List<Task> tasks = _processors.Select(processor => processor.ProcessAsync(klines, result.Items, ct)).ToList();
        await Task.WhenAll(tasks);

        return result;
    }
}
