using Microsoft.EntityFrameworkCore.Storage;
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

        await using var transaction = await _stockRepository.BeginTransactionAsync(ct);

        try
        {
            await _stockRepository.BulkUpsertKlinesAsync(list, ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}