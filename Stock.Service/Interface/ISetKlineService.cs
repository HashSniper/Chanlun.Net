using Stock.Data.Entities;

namespace Stock.Service.Interface;

public interface ISetKlineService
{
    Task SaveKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : KlineBase;
}