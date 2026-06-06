using Stock.Data.Entities;
using Stock.Data.Repositories;
using Stock.Service.Interface;

namespace Stock.Service;

public class SaveTdxCurrentKlineViewService : ISaveTdxCurrentKlineViewService
{
    private readonly IStockRepository _stockRepository;
    public SaveTdxCurrentKlineViewService(IStockRepository repository)
    {
        _stockRepository = repository;
    }
    
    public async Task SaveTdxCurrentKlineView(TdxCurrentKlineView view)
    {
        await _stockRepository.ClearTdxCurrentKlineViewsAsync();
        await _stockRepository.AddTdxCurrentKlineViewAsync(view);
        await _stockRepository.SaveChangesAsync();
    }
}