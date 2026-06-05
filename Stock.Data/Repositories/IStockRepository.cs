using Stock.Data.Entities;

namespace Stock.Data.Repositories;

public interface IStockRepository
{
    #region StockInfo

    Task<StockInfo?> GetStockBySymbolAsync(string symbol, CancellationToken ct = default);
    Task<IEnumerable<StockInfo>> GetStocksAsync(CancellationToken ct = default);
    Task<IEnumerable<StockInfo>> SearchStocksAsync(string keyword, CancellationToken ct = default);
    Task AddStockAsync(StockInfo stock, CancellationToken ct = default);
    Task AddStocksAsync(IEnumerable<StockInfo> stocks, CancellationToken ct = default);
    Task UpdateStockAsync(StockInfo stock, CancellationToken ct = default);
    Task DeleteStockAsync(string symbol, CancellationToken ct = default);

    #endregion

    #region Kline 泛型接口（按周期分表）

    /// <summary>
    /// 查询指定时间范围的K线（泛型，T 为具体周期实体）
    /// </summary>
    Task<IEnumerable<T>> GetKlinesAsync<T>(string symbol, DateTime fromTime, DateTime toTime, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 查询单根K线
    /// </summary>
    Task<T?> GetKlineAsync<T>(string symbol, DateTime tradeTime, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 新增单根K线
    /// </summary>
    Task AddKlineAsync<T>(T kline, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 批量新增K线
    /// </summary>
    Task AddKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 删除指定时间范围的K线
    /// </summary>
    Task DeleteKlinesAsync<T>(string symbol, DateTime fromTime, DateTime toTime, CancellationToken ct = default) where T : class;

    /// <summary>
    /// 保存变更
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    #endregion
}
