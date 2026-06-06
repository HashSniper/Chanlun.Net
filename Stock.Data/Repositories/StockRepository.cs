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
    
    
    public async Task AddKlinesAsync<T>(IEnumerable<T> klines, CancellationToken ct = default) where T : KlineBase
    {
        var now = DateTime.Now;
        foreach (var kline in klines)
        {
            kline.CreatedAt = now;
        }
        await _context.Set<T>().AddRangeAsync(klines, ct);
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
