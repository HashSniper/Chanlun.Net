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

    #region Kline 查询接口

    /// <summary>
    /// 查询指定时间范围的K线（泛型，T 为具体周期实体）
    /// </summary>
    Task<IEnumerable<T>> GetKlinesAsync<T>(string symbol, KlineResolution resolution,DateTime fromTime, DateTime toTime, CancellationToken ct = default) where T : KlineBase;
    
    /// <summary>
    /// 批量新增K线
    /// </summary>
    Task AddKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : KlineBase;

    /// <summary>
    /// 删除指定时间范围的K线
    /// </summary>
    Task DeleteKlinesAsync<T>(string symbol, DateTime fromTime, DateTime toTime, CancellationToken ct = default) where T : KlineBase;

    /// <summary>
    /// 保存变更
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    #endregion

    #region TdxCurrentKlineView

    /// <summary>
    /// 查询通达信当前K线视图（不带条件，仅取第一条）
    /// </summary>
    Task<TdxCurrentKlineView?> GetTdxCurrentKlineViewAsync(CancellationToken ct = default);

    /// <summary>
    /// 新增通达信当前K线视图，保存后返回生成的 Id
    /// </summary>
    Task<long> AddTdxCurrentKlineViewAsync(TdxCurrentKlineView record, CancellationToken ct = default);

    /// <summary>
    /// 清空通达信当前K线视图
    /// </summary>
    Task ClearTdxCurrentKlineViewsAsync(CancellationToken ct = default);

    #endregion
}
