using Microsoft.Extensions.DependencyInjection;
using Stock.Service.Indicators;
using Stock.Service.Interface;

namespace Stock.Service;

public static class ServiceExtensions
{
    public static IServiceCollection AddMyService(this IServiceCollection services)
    {
        services.AddTransient<ISetKlineService, SetKlineService>();
        services.AddTransient<IKLineIndicatorService, KLineIndicatorService>();
        services.AddTransient<IGetTdxCurrentKlineViewService, GetTdxCurrentKlineViewService>();
        services.AddTransient<ISaveTdxCurrentKlineViewService, SaveTdxCurrentKlineViewService>();

        // 指标处理器 IOC 注册
        services.AddTransient<IIndicatorProcessor, MaIndicatorProcessor>();
        services.AddTransient<IIndicatorProcessor, MacdIndicatorProcessor>();
        services.AddTransient<IIndicatorProcessor, KdjIndicatorProcessor>();
        services.AddTransient<IIndicatorProcessor, RsiIndicatorProcessor>();
        services.AddTransient<IIndicatorProcessor, BollIndicatorProcessor>();
        services.AddTransient<IIndicatorProcessor, CandlestickPatternIndicatorProcessor>();
        services.AddTransient<IIndicatorProcessor, VolumeAnalysisIndicatorProcessor>();

        return services;
    }
}
