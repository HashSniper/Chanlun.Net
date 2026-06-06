using Stock.Data.Entities;
using Stock.Data.Repositories;
using Stock.Service.Interface;

namespace Stock.Service;

public class SetKlineService : ISetKlineService
{
    private readonly IStockRepository _stockRepository;

    public SetKlineService(IStockRepository repository)
    {
        _stockRepository = repository;
    }

    public async Task SaveKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : KlineBase
    {
        var list = klines?.ToList();
        if (list == null || list.Count == 0)
            return;
        var resolutions = list.GroupBy(k => k.Resolution);
        if (resolutions.Count() > 1)
        {
            throw new NotSupportedException("Multiple resolutions are not supported");
        }

        // 按 Symbol 分组，分别查询已有数据并过滤
        var groups = list.GroupBy(k => k.Symbol);
        foreach (var group in groups)
        {
            var symbol = group.Key;
            var items = group.ToList();
            var fromTime = items.Min(k => k.TradeTime);
            var toTime = items.Max(k => k.TradeTime);

            // 查询数据库中该 Symbol 在该时间范围内的已有数据
            var existing =
                await _stockRepository.GetKlinesAsync<T>(symbol, resolutions.First().Key, fromTime, toTime, ct);
            var existingKeys = existing.Select(GetKlineKey).ToHashSet();

            // 过滤出数据库中不存在的数据
            var toAdd = items.Where(k => !existingKeys.Contains(GetKlineKey(k))).ToList();
            if (toAdd.Count > 0)
            {
                await _stockRepository.AddKlinesAsync(toAdd, ct);
            }
        }

        await _stockRepository.SaveChangesAsync(ct);
    }

    /// <summary>
    /// 生成 K 线的唯一标识键：Symbol + TradeTime
    /// </summary>
    private static string GetKlineKey<T>(T kline) where T : KlineBase
    {
        return $"{kline.Symbol}_{kline.TradeTime:yyyyMMddHHmmss}";
    }
}