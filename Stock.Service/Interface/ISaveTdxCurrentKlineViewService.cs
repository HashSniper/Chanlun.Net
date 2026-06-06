using Stock.Data.Entities;

namespace Stock.Service.Interface;

public interface ISaveTdxCurrentKlineViewService
{
    Task SaveTdxCurrentKlineView(TdxCurrentKlineView view);
}