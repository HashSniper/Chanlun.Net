using Stock.Data.Entities;
using Stock.Data.Repositories;
using Stock.Service.Interface;

namespace Stock.Service;

public class GetTdxCurrentKlineViewService : IGetTdxCurrentKlineViewService
{
    private readonly IStockRepository _stockRepository;
    public GetTdxCurrentKlineViewService(IStockRepository repository)
    {
        _stockRepository = repository;
    }


    public async Task<TdxCurrentKlineView?> GetTdxCurrentKlineView()
    {
        return await _stockRepository.GetTdxCurrentKlineViewAsync();
    }
}