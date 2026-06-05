using Stock.Data.Entities;
using Microsoft.EntityFrameworkCore;

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

    #region Kline 泛型实现

    public async Task<IEnumerable<T>> GetKlinesAsync<T>(string symbol, DateTime fromTime, DateTime toTime, CancellationToken ct = default)
        where T : class
    {
        var query = _context.Set<T>().AsNoTracking()
            .Where(k => EF.Property<string>(k, "Symbol") == symbol)
            .Where(k => EF.Property<DateTime>(k, "TradeTime") >= fromTime)
            .Where(k => EF.Property<DateTime>(k, "TradeTime") <= toTime)
            .OrderBy(k => EF.Property<DateTime>(k, "TradeTime"));

        return await query.ToListAsync(ct);
    }

    public async Task<T?> GetKlineAsync<T>(string symbol, DateTime tradeTime, CancellationToken ct = default)
        where T : class
    {
        return await _context.Set<T>().AsNoTracking()
            .Where(k => EF.Property<string>(k, "Symbol") == symbol)
            .Where(k => EF.Property<DateTime>(k, "TradeTime") == tradeTime)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddKlineAsync<T>(T kline, CancellationToken ct = default) where T : class
    {
        if (kline is Kline1m k1m) k1m.CreatedAt = DateTime.Now;
        else if (kline is Kline5m k5m) k5m.CreatedAt = DateTime.Now;
        else if (kline is Kline30m k30m) k30m.CreatedAt = DateTime.Now;
        else if (kline is Kline1d k1d) k1d.CreatedAt = DateTime.Now;
        else if (kline is Kline1w k1w) k1w.CreatedAt = DateTime.Now;
        else if (kline is Kline1mo k1mo) k1mo.CreatedAt = DateTime.Now;

        await _context.Set<T>().AddAsync(kline, ct);
    }

    public async Task AddKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : class
    {
        var now = DateTime.Now;
        foreach (var kline in klines)
        {
            if (kline is Kline1m k1m) k1m.CreatedAt = now;
            else if (kline is Kline5m k5m) k5m.CreatedAt = now;
            else if (kline is Kline30m k30m) k30m.CreatedAt = now;
            else if (kline is Kline1d k1d) k1d.CreatedAt = now;
            else if (kline is Kline1w k1w) k1w.CreatedAt = now;
            else if (kline is Kline1mo k1mo) k1mo.CreatedAt = now;
        }
        await _context.Set<T>().AddRangeAsync(klines, ct);
    }

    public async Task DeleteKlinesAsync<T>(string symbol, DateTime fromTime, DateTime toTime, CancellationToken ct = default)
        where T : class
    {
        var klines = await _context.Set<T>()
            .Where(k => EF.Property<string>(k, "Symbol") == symbol)
            .Where(k => EF.Property<DateTime>(k, "TradeTime") >= fromTime)
            .Where(k => EF.Property<DateTime>(k, "TradeTime") <= toTime)
            .ToListAsync(ct);

        _context.Set<T>().RemoveRange(klines);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    #endregion
}
