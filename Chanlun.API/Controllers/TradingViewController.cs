using System.Collections.Concurrent;
using Chanlun.API.Adapter;
using Chanlun.API.Models;
using Chanlun.Lib;
using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;
using Microsoft.AspNetCore.Mvc;

namespace Chanlun.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingViewController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, List<TvKlineBar>> KlineCache = new();

    [HttpPost("chanlun")]
    public IActionResult CalculateChanlun([FromBody] TvChanlunRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return BadRequest(new { error = "Symbol is required" });

        if (request.Bars == null || request.Bars.Count == 0)
            return BadRequest(new { error = "Bars is required" });

        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var bars = request.Bars.OrderBy(b => b.Time).ToList();
        var n = bars.Count;

        try
        {
            var (key, _, _) = PrepareAndCalculate(symbol, bars);
            var result = ChanCalculateResultCache.Get(key);
            if (result == null)
                return StatusCode(500, new { error = "Chanlun calculation failed" });

            var response = ConvertToTvResponse(symbol, n, result);
            KlineCache.AddOrUpdate(symbol, bars, (_, _) => bars);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("push")]
    public IActionResult PushKlines([FromBody] TvChanlunRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol) || request.Bars == null || request.Bars.Count == 0)
            return BadRequest(new { error = "Symbol and Bars are required" });

        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var bars = request.Bars.OrderBy(b => b.Time).ToList();
        KlineCache.AddOrUpdate(symbol, bars, (_, _) => bars);

        return Ok(new { symbol, count = bars.Count, message = "Klines pushed successfully" });
    }

    [HttpGet("config")]
    public IActionResult Config() => Ok(new TvUdfConfig());

    [HttpGet("symbols")]
    public IActionResult Symbols([FromQuery] string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest(new { error = "symbol is required" });

        var s = symbol.Trim().ToUpperInvariant();
        var exchange = s.StartsWith("SZ") ? "SZ" : s.StartsWith("BJ") ? "BJ" : "SH";

        return Ok(new TvUdfSymbolInfo
        {
            Name = s,
            Description = s,
            Exchange = exchange,
            Listed_exchange = exchange,
        });
    }

    [HttpGet("history")]
    public IActionResult History(
        [FromQuery] string symbol,
        [FromQuery] string resolution,
        [FromQuery] long from,
        [FromQuery] long to)
    {
        

        var fromDt = DateTimeOffset.FromUnixTimeSeconds(from).UtcDateTime;
        var toDt = DateTimeOffset.FromUnixTimeSeconds(to).UtcDateTime;

        var filtered = TradingViewAdapter.GetKlineBars(symbol, resolution, fromDt, toDt);

        if (filtered.Count == 0)
            return Ok(new TvUdfHistory { S = "no_data" });

        return Ok(new TvUdfHistory
        {
            S = "ok",
            T = filtered.Select(b => new DateTimeOffset(b.Time).ToUnixTimeSeconds()).ToArray(),
            O = filtered.Select(b => b.Open).ToArray(),
            H = filtered.Select(b => b.High).ToArray(),
            L = filtered.Select(b => b.Low).ToArray(),
            C = filtered.Select(b => b.Close).ToArray(),
            V = filtered.Select(b => b.Volume).ToArray(),
        });
    }

    [HttpGet("time")]
    public IActionResult Time()
    {
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Content(unixTime.ToString(), "text/plain");
    }

    [HttpGet("search")]
    public IActionResult Search(
        [FromQuery] string query,
        [FromQuery] string? type,
        [FromQuery] string? exchange,
        [FromQuery] int limit = 30)
    {
        var results = new List<TvUdfSearchResultItem>();

        foreach (var sym in KlineCache.Keys)
        {
            if (sym.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                var ex = sym.StartsWith("SZ") ? "SZ" : sym.StartsWith("BJ") ? "BJ" : "SH";
                results.Add(new TvUdfSearchResultItem
                {
                    Symbol = sym,
                    Full_name = $"{ex}:{sym}",
                    Description = sym,
                    Exchange = ex,
                    Type = "stock"
                });
            }

            if (results.Count >= limit)
                break;
        }

        return Ok(results);
    }

    private static DateTime ParseTvTime(string timeStr)
    {
        if (!string.IsNullOrWhiteSpace(timeStr) && timeStr.Length >= 8
            && int.TryParse(timeStr[..4], out int year)
            && int.TryParse(timeStr[4..6], out int month)
            && int.TryParse(timeStr[6..8], out int day))
        {
            return new DateTime(year, month, day);
        }
        if (DateTime.TryParse(timeStr, out var dt))
        {
            return dt;
        }
        throw new FormatException($"Invalid time format: {timeStr}, expected yyyyMMdd");
    }

    private static (decimal key, decimal[] highs, decimal[] lows) PrepareAndCalculate(string symbol, List<TvKlineBar> bars)
    {
        var n = bars.Count;

        var dates = new decimal[n];
        var times = new decimal[n];
        var highs = new decimal[n];
        var lows = new decimal[n];

        for (int i = 0; i < n; i++)
        {
            var bar = bars[i];
            var dt = bar.Time;
            dates[i] = (dt.Year - 1900) * 10000 + dt.Month * 100 + dt.Day;
            times[i] = dt.Hour * 10000 + dt.Minute * 100 + dt.Second;
            highs[i] = bar.High;
            lows[i] = bar.Low;
        }

        var codeValue = Math.Abs(symbol.GetHashCode()) % 1000000;
        var code = new decimal[] { codeValue };

        // var keyArray = StockTimeCache.Set(code, dates, times);
        // var key = keyArray[0];
        

        return (1, highs, lows);
    }

    private static TvChanlunResponse ConvertToTvResponse(string symbol, int barCount, ChanCalculateResult result)
    {
        var response = new TvChanlunResponse
        {
            Symbol = symbol,
            BarCount = barCount
        };

        if (result.BiList.IsNotNullOrEmpty())
        {
            foreach (var bi in result.BiList)
            {
                var startUnit = bi.StartChanKLine.PeakUnit;
                var endUnit = bi.EndChanKLine.PeakUnit;
                var startPrice = bi.DIR.IsUp() ? bi.StartChanKLine.Low : bi.StartChanKLine.High;
                var endPrice = bi.DIR.IsUp() ? bi.EndChanKLine.High : bi.EndChanKLine.Low;

                response.BiList.Add(new TvBiItem
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    StartPrice = startPrice,
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    EndPrice = endPrice,
                    Direction = bi.DIR.IsUp() ? "up" : "down"
                });
            }
        }

        if (result.SegList.IsNotNullOrEmpty())
        {
            foreach (var seg in result.SegList)
            {
                var startUnit = seg.StartBi.StartChanKLine.PeakUnit;
                var endUnit = seg.EndBi.EndChanKLine.PeakUnit;
                var startPrice = seg.DIR.IsUp() ? seg.Low : seg.High;
                var endPrice = seg.DIR.IsUp() ? seg.High : seg.Low;

                response.SegList.Add(new TvSegItem
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    StartPrice = startPrice,
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    EndPrice = endPrice,
                    Direction = seg.DIR.IsUp() ? "up" : "down"
                });
            }
        }

        if (result.BiPivotList.IsNotNullOrEmpty())
        {
            foreach (var pivot in result.BiPivotList)
            {
                var startUnit = pivot.Segments[0].StartChanKLine.PeakUnit;
                var endUnit = pivot.Segments[^1].EndChanKLine.PeakUnit;

                response.BiPivotList.Add(new TvPivotItem
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    ZG = pivot.ZG,
                    ZD = pivot.ZD,
                    GG = pivot.GG,
                    DD = pivot.DD,
                    Type = "bi",
                    Level = pivot.Level
                });
            }
        }

        if (result.SegPivotList.IsNotNullOrEmpty())
        {
            foreach (var pivot in result.SegPivotList)
            {
                var startUnit = pivot.Segments[0].StartBi.StartChanKLine.PeakUnit;
                var endUnit = pivot.Segments[^1].EndBi.EndChanKLine.PeakUnit;

                response.SegPivotList.Add(new TvPivotItem
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    ZG = pivot.ZG,
                    ZD = pivot.ZD,
                    GG = pivot.GG,
                    DD = pivot.DD,
                    Type = "seg",
                    Level = pivot.Level
                });
            }
        }

        if (result.LineList.IsNotNullOrEmpty())
        {
            foreach (var kline in result.LineList)
            {
                if (kline.CombinedUnits.Count == 0) continue;

                var startUnit = kline.CombinedUnits[0];
                var endUnit = kline.CombinedUnits[^1];

                response.MergedKLines.Add(new TvMergedKLine
                {
                    StartTime = new DateTimeOffset(startUnit.Time).ToUnixTimeMilliseconds(),
                    EndTime = new DateTimeOffset(endUnit.Time).ToUnixTimeMilliseconds(),
                    High = kline.High,
                    Low = kline.Low,
                    Direction = kline.DIR == ChanDir.UP ? "up" : kline.DIR == ChanDir.DOWN ? "down" : "combine"
                });
            }
        }

        return response;
    }
}
