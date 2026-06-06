using Stock.Data.Entities;
using Stock.Data.Repositories;
using Stock.Service.Interface;

namespace Stock.Service;

public class GetKLineService : IGetKLineService
{
    private readonly IStockRepository _stockRepository;
    public GetKLineService(IStockRepository repository)
    {
        _stockRepository = repository;
    }

    public async Task<List<KlineBase>> GetKlinesAsync(GetKLineQuery query)
    {
        return (await _stockRepository.GetKlinesAsync<KlineBase>(query.Symbol, query.Resolution, query.FromTime, query.ToTime)).ToList();
    }
}