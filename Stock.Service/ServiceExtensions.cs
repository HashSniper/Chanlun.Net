using Microsoft.Extensions.DependencyInjection;
using Stock.Service.Interface;

namespace Stock.Service;

public static class ServiceExtensions
{
    public static IServiceCollection AddMyService(this IServiceCollection services)
    {
        services.AddTransient<ISetStockDataService, SetStockDataService>();
        return services;
    }
}