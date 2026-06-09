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

        // 按 Symbol 分组，分别查询已有数据
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
            var existingDict = existing.ToDictionary(GetKlineKey);

            var toAdd = new List<T>();
            var toUpdate = new List<T>();

            foreach (var item in items)
            {
                var key = GetKlineKey(item);
                if (existingDict.TryGetValue(key, out var existingItem))
                {
                    if (HasDifference(item, existingItem))
                    {
                        // 复制新值到已有实体，保留 Id
                        existingItem.Open = item.Open;
                        existingItem.High = item.High;
                        existingItem.Low = item.Low;
                        existingItem.Close = item.Close;
                        existingItem.Volume = item.Volume;
                        existingItem.Amount = item.Amount;
                        toUpdate.Add(existingItem);
                    }
                }
                else
                {
                    toAdd.Add(item);
                }
            }

            if (toAdd.Count > 0)
            {
                await _stockRepository.AddKlinesAsync(toAdd, ct);
            }
            if (toUpdate.Count > 0)
            {
                await _stockRepository.UpdateKlinesAsync(toUpdate, ct);
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

    /// <summary>
    /// 比较两个 K 线的 OHLCVA 是否有差异
    /// </summary>
    private static bool HasDifference<T>(T incoming, T existing) where T : KlineBase
    {
        return incoming.Open != existing.Open
            || incoming.High != existing.High
            || incoming.Low != existing.Low
            || incoming.Close != existing.Close
            || incoming.Volume != existing.Volume
            || incoming.Amount != existing.Amount;
    }
}