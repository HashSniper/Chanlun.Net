using Stock.Data.Entities;

namespace Stock.Service.Interface;

public interface IGetTdxCurrentKlineViewService
{
    Task<TdxCurrentKlineView?> GetTdxCurrentKlineView();
}