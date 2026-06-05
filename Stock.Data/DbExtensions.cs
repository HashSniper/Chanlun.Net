using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stock.Data.Repositories;

namespace Stock.Data;

public static class DbExtensions
{
    public static IServiceCollection AddDb(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 注册 Repository
        services.AddScoped<IStockRepository, StockRepository>();
        return services;
    }
}