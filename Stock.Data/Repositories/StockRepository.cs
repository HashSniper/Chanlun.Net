using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Stock.Data.Entities;

namespace Stock.Data.Repositories;

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;

    public StockRepository(AppDbContext context)
    {
        _context = context;
    }

    #region StockInfo

    public Task<StockInfo?> GetStockBySymbolAsync(string symbol, CancellationToken ct = default)
    {
        return _context.StockInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Symbol == symbol, ct);
    }

    public Task<IEnumerable<StockInfo>> GetStocksAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IEnumerable<StockInfo>>(
            _context.StockInfos.AsNoTracking().AsEnumerable());
    }

    public Task<IEnumerable<StockInfo>> SearchStocksAsync(string keyword, CancellationToken ct = default)
    {
        var query = _context.StockInfos
            .AsNoTracking()
            .Where(s => s.Symbol.Contains(keyword) || s.Name.Contains(keyword));
        return Task.FromResult<IEnumerable<StockInfo>>(query.AsEnumerable());
    }

    public async Task AddStockAsync(StockInfo stock, CancellationToken ct = default)
    {
        stock.CreatedAt = DateTime.Now;
        stock.UpdatedAt = DateTime.Now;
        await _context.StockInfos.AddAsync(stock, ct);
    }

    public async Task AddStocksAsync(IEnumerable<StockInfo> stocks, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        foreach (var stock in stocks)
        {
            stock.CreatedAt = now;
            stock.UpdatedAt = now;
        }
        await _context.StockInfos.AddRangeAsync(stocks, ct);
    }

    public Task UpdateStockAsync(StockInfo stock, CancellationToken ct = default)
    {
        stock.UpdatedAt = DateTime.Now;
        _context.StockInfos.Update(stock);
        return Task.CompletedTask;
    }

    public async Task DeleteStockAsync(string symbol, CancellationToken ct = default)
    {
        var stock = await _context.StockInfos.FirstOrDefaultAsync(s => s.Symbol == symbol, ct);
        if (stock != null)
        {
            _context.StockInfos.Remove(stock);
        }
    }

    #endregion

    #region Kline 查询实现
    
    public async Task<IEnumerable<T>> GetKlinesAsync<T>(string symbol,KlineResolution resolution, DateTime fromTime, DateTime toTime, CancellationToken ct = default)
        where T : KlineBase
    {
         return resolution switch
        {
            KlineResolution.Minute1 => (await _context.Kline1m.AsNoTracking()
                .Where(k => k.Symbol == symbol && k.TradeTime >= fromTime && k.TradeTime <= toTime)
                .OrderBy(k => k.TradeTime).ToListAsync(ct)).Cast<T>().ToList(),
            KlineResolution.Minute5 => (await _context.Kline5m.AsNoTracking()
                .Where(k => k.Symbol == symbol && k.TradeTime >= fromTime && k.TradeTime <= toTime)
                .OrderBy(k => k.TradeTime).ToListAsync(ct)).Cast<T>().ToList(),
            KlineResolution.Minute15 => (await _context.Kline15m.AsNoTracking()
                .Where(k => k.Symbol == symbol && k.TradeTime >= fromTime && k.TradeTime <= toTime)
                .OrderBy(k => k.TradeTime).ToListAsync(ct)).Cast<T>().ToList(),
            KlineResolution.Minute30 => (await _context.Kline30m.AsNoTracking()
                .Where(k => k.Symbol == symbol && k.TradeTime >= fromTime && k.TradeTime <= toTime)
                .OrderBy(k => k.TradeTime).ToListAsync(ct)).Cast<T>().ToList(),
            KlineResolution.Minute60 => (await _context.Kline60m.AsNoTracking()
                .Where(k => k.Symbol == symbol && k.TradeTime >= fromTime && k.TradeTime <= toTime)
                .OrderBy(k => k.TradeTime).ToListAsync(ct)).Cast<T>().ToList(),
            KlineResolution.Day => (await _context.Kline1d.AsNoTracking()
                .Where(k => k.Symbol == symbol && k.TradeTime >= fromTime && k.TradeTime <= toTime)
                .OrderBy(k => k.TradeTime).ToListAsync(ct)).Cast<T>().ToList(),
            KlineResolution.Week => (await _context.Kline1w.AsNoTracking()
                .Where(k => k.Symbol == symbol && k.TradeTime >= fromTime && k.TradeTime <= toTime)
                .OrderBy(k => k.TradeTime).ToListAsync(ct)).Cast<T>().ToList(),
            KlineResolution.Month => (await _context.Kline1mo.AsNoTracking()
                .Where(k => k.Symbol == symbol && k.TradeTime >= fromTime && k.TradeTime <= toTime)
                .OrderBy(k => k.TradeTime).ToListAsync(ct)).Cast<T>().ToList(),
            _ => throw new ArgumentException($"Unsupported resolution: {resolution}")
        };
    }
    
    
    private (string tableName, string schema) ResolveKlineTableName<T>(IReadOnlyList<T> list) where T : KlineBase
    {
        var actualType = typeof(T);
        var entityType = _context.Model.FindEntityType(actualType);
        var tableName = entityType?.GetTableName();
        var schema = entityType?.GetSchema();

        if (string.IsNullOrEmpty(tableName) && list.Count > 0)
        {
            // TPC 基类（如 KlineBase）没有对应表，需从 Resolution 推断具体子类
            actualType = list[0].Resolution switch
            {
                KlineResolution.Minute1 => typeof(Kline1m),
                KlineResolution.Minute5 => typeof(Kline5m),
                KlineResolution.Minute15 => typeof(Kline15m),
                KlineResolution.Minute30 => typeof(Kline30m),
                KlineResolution.Minute60 => typeof(Kline60m),
                KlineResolution.Day => typeof(Kline1d),
                KlineResolution.Week => typeof(Kline1w),
                KlineResolution.Month => typeof(Kline1mo),
                _ => actualType
            };
            entityType = _context.Model.FindEntityType(actualType);
            tableName = entityType?.GetTableName();
            schema = entityType?.GetSchema();
        }

        return (tableName ?? string.Empty, schema ?? string.Empty);
    }

    public async Task AddKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : KlineBase
    {
        var list = klines?.ToList();
        if (list == null || list.Count == 0)
            return;

        var now = DateTime.Now;
        foreach (var kline in list)
        {
            kline.CreatedAt = now;
        }

        // 小批量仍走 EF ChangeTracker，减少 SqlBulkCopy 的固定开销
        if (list.Count <= 100)
        {
            await _context.Set<T>().AddRangeAsync(list, ct);
            return;
        }

        var (tableName, schema) = ResolveKlineTableName(list);

        if (string.IsNullOrEmpty(tableName))
        {
            await _context.Set<T>().AddRangeAsync(list, ct);
            return;
        }

        var connection = _context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(ct);

        try
        {
            var destinationTableName = string.IsNullOrEmpty(schema)
                ? $"[{tableName}]"
                : $"[{schema}].[{tableName}]";

            var sqlTransaction = _context.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction;

            using var bulkCopy = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.Default, sqlTransaction);
            bulkCopy.DestinationTableName = destinationTableName;
            bulkCopy.BatchSize = 5000;
            bulkCopy.BulkCopyTimeout = 300;

            var dataTable = new DataTable();
            dataTable.Columns.Add("Symbol", typeof(string));
            dataTable.Columns.Add("TradeTime", typeof(DateTime));
            dataTable.Columns.Add("Open", typeof(decimal));
            dataTable.Columns.Add("High", typeof(decimal));
            dataTable.Columns.Add("Low", typeof(decimal));
            dataTable.Columns.Add("Close", typeof(decimal));
            dataTable.Columns.Add("Volume", typeof(decimal));
            dataTable.Columns.Add("Amount", typeof(decimal));
            dataTable.Columns.Add("CreatedAt", typeof(DateTime));

            foreach (var item in list)
            {
                dataTable.Rows.Add(
                    item.Symbol,
                    item.TradeTime,
                    item.Open,
                    item.High,
                    item.Low,
                    item.Close,
                    item.Volume,
                    item.Amount,
                    item.CreatedAt);
            }

            await bulkCopy.WriteToServerAsync(dataTable, ct);
        }
        finally
        {
            if (!wasOpen && connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return _context.Database.BeginTransactionAsync(ct);
    }

    public async Task BulkUpsertKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : KlineBase
    {
        var list = klines?.ToList();
        if (list == null || list.Count == 0)
            return;

        var now = DateTime.Now;
        foreach (var kline in list)
        {
            kline.CreatedAt = now;
        }

        var (tableName, schema) = ResolveKlineTableName(list);

        if (string.IsNullOrEmpty(tableName))
            throw new InvalidOperationException($"Unable to determine table name for entity {typeof(T).Name}");

        var connection = _context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(ct);

        var sqlTransaction = _context.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction;
        var ownsTransaction = false;

        try
        {
            if (sqlTransaction == null)
            {
                sqlTransaction = ((SqlConnection)connection).BeginTransaction();
                ownsTransaction = true;
            }

            var destinationTableName = string.IsNullOrEmpty(schema)
                ? $"[{tableName}]"
                : $"[{schema}].[{tableName}]";

            using var command = new SqlCommand();
            command.Connection = (SqlConnection)connection;
            command.Transaction = sqlTransaction;

            // 创建与目标表结构一致的临时表（不含自增 Id）
            command.CommandText = $@"
                SELECT TOP 0 Symbol, TradeTime, [Open], High, Low, [Close], Volume, Amount, CreatedAt
                INTO #TempKlines
                FROM {destinationTableName}";
            await command.ExecuteNonQueryAsync(ct);

            // BulkCopy 到临时表
            using var bulkCopy = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.Default, sqlTransaction);
            bulkCopy.DestinationTableName = "#TempKlines";
            bulkCopy.BatchSize = 5000;
            bulkCopy.BulkCopyTimeout = 300;

            var dataTable = new DataTable();
            dataTable.Columns.Add("Symbol", typeof(string));
            dataTable.Columns.Add("TradeTime", typeof(DateTime));
            dataTable.Columns.Add("Open", typeof(decimal));
            dataTable.Columns.Add("High", typeof(decimal));
            dataTable.Columns.Add("Low", typeof(decimal));
            dataTable.Columns.Add("Close", typeof(decimal));
            dataTable.Columns.Add("Volume", typeof(decimal));
            dataTable.Columns.Add("Amount", typeof(decimal));
            dataTable.Columns.Add("CreatedAt", typeof(DateTime));

            foreach (var item in list)
            {
                dataTable.Rows.Add(
                    item.Symbol,
                    item.TradeTime,
                    item.Open,
                    item.High,
                    item.Low,
                    item.Close,
                    item.Volume,
                    item.Amount,
                    item.CreatedAt);
            }

            await bulkCopy.WriteToServerAsync(dataTable, ct);

            // 执行 MERGE
            command.CommandText = $@"
                MERGE INTO {destinationTableName} AS target
                USING #TempKlines AS source
                ON target.Symbol = source.Symbol AND target.TradeTime = source.TradeTime
                WHEN MATCHED AND (
                    target.[Open] <> source.[Open] OR
                    target.High <> source.High OR
                    target.Low <> source.Low OR
                    target.[Close] <> source.[Close] OR
                    target.Volume <> source.Volume OR
                    target.Amount <> source.Amount
                ) THEN
                    UPDATE SET
                        target.[Open] = source.[Open],
                        target.High = source.High,
                        target.Low = source.Low,
                        target.[Close] = source.[Close],
                        target.Volume = source.Volume,
                        target.Amount = source.Amount
                WHEN NOT MATCHED THEN
                    INSERT (Symbol, TradeTime, [Open], High, Low, [Close], Volume, Amount, CreatedAt)
                    VALUES (source.Symbol, source.TradeTime, source.[Open], source.High, source.Low, source.[Close], source.Volume, source.Amount, source.CreatedAt);";

            await command.ExecuteNonQueryAsync(ct);

            if (ownsTransaction)
            {
                sqlTransaction.Commit();
            }
        }
        catch
        {
            if (ownsTransaction && sqlTransaction != null)
            {
                sqlTransaction.Rollback();
            }
            throw;
        }
        finally
        {
            if (!wasOpen && connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }

    public Task UpdateKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : KlineBase
    {
        _context.Set<T>().UpdateRange(klines);
        return Task.CompletedTask;
    }

    public async Task DeleteKlinesAsync<T>(string symbol, DateTime fromTime, DateTime toTime, CancellationToken ct = default)
        where T : KlineBase
    {
        var klines = await _context.Set<T>()
            .Where(k => k.Symbol == symbol)
            .Where(k => k.TradeTime >= fromTime)
            .Where(k => k.TradeTime <= toTime)
            .ToListAsync(ct);

        _context.Set<T>().RemoveRange(klines);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    #endregion

    #region TdxCurrentKlineView

    public Task<TdxCurrentKlineView?> GetTdxCurrentKlineViewAsync(CancellationToken ct = default)
    {
        return _context.TdxCurrentKlineViews
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<long> AddTdxCurrentKlineViewAsync(TdxCurrentKlineView record, CancellationToken ct = default)
    {
        record.CreatedAt = DateTime.Now;
        await _context.TdxCurrentKlineViews.AddAsync(record, ct);
        await _context.SaveChangesAsync(ct);
        return record.Id;
    }

    public async Task ClearTdxCurrentKlineViewsAsync(CancellationToken ct = default)
    {
        await _context.TdxCurrentKlineViews.ExecuteDeleteAsync(ct);
    }

    #endregion
}
