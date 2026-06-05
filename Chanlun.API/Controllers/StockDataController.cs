using Stock.Data.Entities;
using Stock.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Chanlun.API.Controllers;

/// <summary>
/// 股票数据存取接口（基于 EF Core + SQL Server，K线按周期分表）
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StockDataController : ControllerBase
{
    private readonly IStockRepository _repo;

    public StockDataController(IStockRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// 根据 resolution 字符串解析为对应的 Kline 实体类型
    /// </summary>
    private static Type ResolveKlineType(string resolution) => resolution.ToUpperInvariant() switch
    {
        "1" => typeof(Kline1m),
        "5" => typeof(Kline5m),
        "15" => typeof(Kline15m),
        "30" => typeof(Kline30m),
        "60" => typeof(Kline60m),
        "D" => typeof(Kline1d),
        "W" => typeof(Kline1w),
        "M" => typeof(Kline1mo),
        _ => throw new ArgumentException($"Unsupported resolution: {resolution}. Supported: 1, 5, 15, 30, 60, D, W, M")
    };

    #region 股票基本信息

    /// <summary>查询股票列表</summary>
    [HttpGet("stocks")]
    public async Task<IActionResult> GetStocks(CancellationToken ct)
    {
        var stocks = await _repo.GetStocksAsync(ct);
        return Ok(stocks);
    }

    /// <summary>按代码查询单只股票</summary>
    [HttpGet("stocks/{symbol}")]
    public async Task<IActionResult> GetStock(string symbol, CancellationToken ct)
    {
        var stock = await _repo.GetStockBySymbolAsync(symbol.ToUpperInvariant(), ct);
        return stock == null ? NotFound(new { error = "Stock not found" }) : Ok(stock);
    }

    /// <summary>搜索股票（代码或名称模糊匹配）</summary>
    [HttpGet("stocks/search")]
    public async Task<IActionResult> SearchStocks([FromQuery] string keyword, CancellationToken ct)
    {
        var stocks = await _repo.SearchStocksAsync(keyword, ct);
        return Ok(stocks);
    }

    /// <summary>新增股票</summary>
    [HttpPost("stocks")]
    public async Task<IActionResult> AddStock([FromBody] StockInfo stock, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(stock.Symbol))
            return BadRequest(new { error = "Symbol is required" });

        stock.Symbol = stock.Symbol.Trim().ToUpperInvariant();
        await _repo.AddStockAsync(stock, ct);
        await _repo.SaveChangesAsync(ct);
        return Ok(new { message = "Stock added", symbol = stock.Symbol });
    }

    /// <summary>批量新增股票</summary>
    [HttpPost("stocks/batch")]
    public async Task<IActionResult> AddStocks([FromBody] List<StockInfo> stocks, CancellationToken ct)
    {
        if (stocks == null || stocks.Count == 0)
            return BadRequest(new { error = "Stocks list is empty" });

        foreach (var s in stocks) s.Symbol = s.Symbol.Trim().ToUpperInvariant();
        await _repo.AddStocksAsync(stocks, ct);
        await _repo.SaveChangesAsync(ct);
        return Ok(new { message = "Stocks added", count = stocks.Count });
    }

    /// <summary>更新股票信息</summary>
    [HttpPut("stocks/{symbol}")]
    public async Task<IActionResult> UpdateStock(string symbol, [FromBody] StockInfo stock, CancellationToken ct)
    {
        var existing = await _repo.GetStockBySymbolAsync(symbol.ToUpperInvariant(), ct);
        if (existing == null)
            return NotFound(new { error = "Stock not found" });

        existing.Name = stock.Name;
        existing.Exchange = stock.Exchange;
        existing.Type = stock.Type;
        await _repo.UpdateStockAsync(existing, ct);
        await _repo.SaveChangesAsync(ct);
        return Ok(new { message = "Stock updated", symbol });
    }

    /// <summary>删除股票</summary>
    [HttpDelete("stocks/{symbol}")]
    public async Task<IActionResult> DeleteStock(string symbol, CancellationToken ct)
    {
        await _repo.DeleteStockAsync(symbol.ToUpperInvariant(), ct);
        await _repo.SaveChangesAsync(ct);
        return Ok(new { message = "Stock deleted", symbol });
    }

    #endregion

    #region K线数据（按周期分表）

    /// <summary>查询K线范围</summary>
    [HttpGet("klines/{symbol}/{resolution}")]
    public async Task<IActionResult> GetKlines(
        string symbol,
        string resolution,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from == default || to == default)
            return BadRequest(new { error = "from and to query params are required" });

        var sym = symbol.ToUpperInvariant();

        return resolution.ToUpperInvariant() switch
        {
            "1" => Ok(await _repo.GetKlinesAsync<Kline1m>(sym, from, to, ct)),
            "5" => Ok(await _repo.GetKlinesAsync<Kline5m>(sym, from, to, ct)),
            "15" => Ok(await _repo.GetKlinesAsync<Kline15m>(sym, from, to, ct)),
            "30" => Ok(await _repo.GetKlinesAsync<Kline30m>(sym, from, to, ct)),
            "60" => Ok(await _repo.GetKlinesAsync<Kline60m>(sym, from, to, ct)),
            "D" => Ok(await _repo.GetKlinesAsync<Kline1d>(sym, from, to, ct)),
            "W" => Ok(await _repo.GetKlinesAsync<Kline1w>(sym, from, to, ct)),
            "M" => Ok(await _repo.GetKlinesAsync<Kline1mo>(sym, from, to, ct)),
            _ => BadRequest(new { error = $"Unsupported resolution: {resolution}. Supported: 1, 5, 15, 30, 60, D, W, M" })
        };
    }

    /// <summary>查询单根K线</summary>
    [HttpGet("klines/{symbol}/{resolution}/{tradeTime:datetime}")]
    public async Task<IActionResult> GetKline(
        string symbol,
        string resolution,
        DateTime tradeTime,
        CancellationToken ct)
    {
        var sym = symbol.ToUpperInvariant();

        return resolution.ToUpperInvariant() switch
        {
            "1" => Ok(await _repo.GetKlineAsync<Kline1m>(sym, tradeTime, ct)),
            "5" => Ok(await _repo.GetKlineAsync<Kline5m>(sym, tradeTime, ct)),
            "15" => Ok(await _repo.GetKlineAsync<Kline15m>(sym, tradeTime, ct)),
            "30" => Ok(await _repo.GetKlineAsync<Kline30m>(sym, tradeTime, ct)),
            "60" => Ok(await _repo.GetKlineAsync<Kline60m>(sym, tradeTime, ct)),
            "D" => Ok(await _repo.GetKlineAsync<Kline1d>(sym, tradeTime, ct)),
            "W" => Ok(await _repo.GetKlineAsync<Kline1w>(sym, tradeTime, ct)),
            "M" => Ok(await _repo.GetKlineAsync<Kline1mo>(sym, tradeTime, ct)),
            _ => BadRequest(new { error = $"Unsupported resolution: {resolution}. Supported: 1, 5, 15, 30, 60, D, W, M" })
        };
    }

    /// <summary>新增单根K线</summary>
    [HttpPost("klines/{resolution}")]
    public async Task<IActionResult> AddKline(string resolution, [FromBody] object kline, CancellationToken ct)
    {
        return resolution.ToUpperInvariant() switch
        {
            "1" when kline is Kline1m k1m => await AddKlineInternal(k1m, ct),
            "5" when kline is Kline5m k5m => await AddKlineInternal(k5m, ct),
            "15" when kline is Kline15m k15m => await AddKlineInternal(k15m, ct),
            "30" when kline is Kline30m k30m => await AddKlineInternal(k30m, ct),
            "60" when kline is Kline60m k60m => await AddKlineInternal(k60m, ct),
            "D" when kline is Kline1d k1d => await AddKlineInternal(k1d, ct),
            "W" when kline is Kline1w k1w => await AddKlineInternal(k1w, ct),
            "M" when kline is Kline1mo k1mo => await AddKlineInternal(k1mo, ct),
            _ => BadRequest(new { error = $"Unsupported resolution or mismatched entity type: {resolution}" })
        };
    }

    private async Task<IActionResult> AddKlineInternal<T>(T kline, CancellationToken ct) where T : class
    {
        if (kline == null)
            return BadRequest(new { error = "Kline is null" });

        var symbolProp = typeof(T).GetProperty("Symbol");
        var symbolValue = symbolProp?.GetValue(kline) as string;
        if (string.IsNullOrWhiteSpace(symbolValue))
            return BadRequest(new { error = "Symbol is required" });

        symbolProp?.SetValue(kline, symbolValue.Trim().ToUpperInvariant());
        await _repo.AddKlineAsync(kline, ct);
        await _repo.SaveChangesAsync(ct);
        return Ok(new { message = "Kline added", type = typeof(T).Name });
    }

    /// <summary>批量新增K线</summary>
    [HttpPost("klines/{resolution}/batch")]
    public async Task<IActionResult> AddKlines(string resolution, [FromBody] object klines, CancellationToken ct)
    {
        return resolution.ToUpperInvariant() switch
        {
            "1" when klines is List<Kline1m> list => await AddKlinesInternal(list, ct),
            "5" when klines is List<Kline5m> list => await AddKlinesInternal(list, ct),
            "15" when klines is List<Kline15m> list => await AddKlinesInternal(list, ct),
            "30" when klines is List<Kline30m> list => await AddKlinesInternal(list, ct),
            "60" when klines is List<Kline60m> list => await AddKlinesInternal(list, ct),
            "D" when klines is List<Kline1d> list => await AddKlinesInternal(list, ct),
            "W" when klines is List<Kline1w> list => await AddKlinesInternal(list, ct),
            "M" when klines is List<Kline1mo> list => await AddKlinesInternal(list, ct),
            _ => BadRequest(new { error = $"Unsupported resolution or mismatched entity list type: {resolution}" })
        };
    }

    private async Task<IActionResult> AddKlinesInternal<T>(List<T> klines, CancellationToken ct) where T : class
    {
        if (klines == null || klines.Count == 0)
            return BadRequest(new { error = "Klines list is empty" });

        var symbolProp = typeof(T).GetProperty("Symbol");
        foreach (var k in klines)
        {
            var symbolValue = symbolProp?.GetValue(k) as string;
            if (!string.IsNullOrWhiteSpace(symbolValue))
                symbolProp?.SetValue(k, symbolValue.Trim().ToUpperInvariant());
        }

        await _repo.AddKlinesAsync(klines, ct);
        await _repo.SaveChangesAsync(ct);
        return Ok(new { message = "Klines added", count = klines.Count, type = typeof(T).Name });
    }

    /// <summary>删除指定时间范围的K线</summary>
    [HttpDelete("klines/{symbol}/{resolution}")]
    public async Task<IActionResult> DeleteKlines(
        string symbol,
        string resolution,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var sym = symbol.ToUpperInvariant();

        return resolution.ToUpperInvariant() switch
        {
            "1" => await DeleteKlinesInternal<Kline1m>(sym, from, to, ct),
            "5" => await DeleteKlinesInternal<Kline5m>(sym, from, to, ct),
            "15" => await DeleteKlinesInternal<Kline15m>(sym, from, to, ct),
            "30" => await DeleteKlinesInternal<Kline30m>(sym, from, to, ct),
            "60" => await DeleteKlinesInternal<Kline60m>(sym, from, to, ct),
            "D" => await DeleteKlinesInternal<Kline1d>(sym, from, to, ct),
            "W" => await DeleteKlinesInternal<Kline1w>(sym, from, to, ct),
            "M" => await DeleteKlinesInternal<Kline1mo>(sym, from, to, ct),
            _ => BadRequest(new { error = $"Unsupported resolution: {resolution}. Supported: 1, 5, 15, 30, 60, D, W, M" })
        };
    }

    private async Task<IActionResult> DeleteKlinesInternal<T>(string symbol, DateTime from, DateTime to, CancellationToken ct) where T : class
    {
        await _repo.DeleteKlinesAsync<T>(symbol, from, to, ct);
        await _repo.SaveChangesAsync(ct);
        return Ok(new { message = "Klines deleted", symbol, type = typeof(T).Name, from, to });
    }

    #endregion
}
