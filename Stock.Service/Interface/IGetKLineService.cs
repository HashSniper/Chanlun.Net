using Stock.Data.Entities;

namespace Stock.Service.Interface;

public interface IGetKLineService
{
    Task<List<KlineBase>> GetKlinesAsync(GetKLineQuery query);
}