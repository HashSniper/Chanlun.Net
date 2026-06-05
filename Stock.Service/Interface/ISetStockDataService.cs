using Stock.Data.Entities;

namespace Stock.Service.Interface;

public interface ISetStockDataService
{
    Task SaveKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : KlineBase;
}