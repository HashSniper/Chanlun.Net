using Microsoft.Extensions.DependencyInjection;
using Stock.Service.Interface;

namespace Stock.Service;

public static class ServiceExtensions
{
    public static IServiceCollection AddMyService(this IServiceCollection services)
    {
        services.AddTransient<ISetKlineService, SetKlineService>();
        services.AddTransient<IGetKLineService, GetKLineService>();
        services.AddTransient<IGetTdxCurrentKlineViewService, GetTdxCurrentKlineViewService>();
        services.AddTransient<ISaveTdxCurrentKlineViewService, SaveTdxCurrentKlineViewService>();
        return services;
    }
}