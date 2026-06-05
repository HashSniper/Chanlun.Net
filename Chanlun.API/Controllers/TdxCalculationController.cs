using Chanlun.API.Adapter;
using Chanlun.API.Models;
using Chanlun.Lib;
using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.Memory;
using Chanlun.Lib.StockIndicators;
using Microsoft.AspNetCore.Mvc;
using Stock.Service.Interface;

namespace Chanlun.API.Controllers;

[ApiController]
[Route("api/calculation")]
public class TdxCalculationController : ControllerBase
{
    private readonly ISetStockDataService _setStockDataService;

    public TdxCalculationController(ISetStockDataService setStockDataService)
    {
        _setStockDataService = setStockDataService;
    }

    /// <summary>
    /// 1. 生成key 已经记录所有的k 线时间
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("setstocktime")]
    public IActionResult SetStockTime([FromBody] CalcRequest request)
    {
        var result = KLineDataPopulator.PopulateTradeTime(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    }

    /// <summary>
    /// 2. 将缠论所有的信息都处理
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("createchan")]
    public IActionResult CreateChan([FromBody] CalcRequest request)
    {
        ChanCalculator.Calculate(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = [] });
    }
    
    /// <summary>
    /// 3. 获取笔相关
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("bilist")]
    public IActionResult CreateBi([FromBody] CalcRequest request)
    {
        var result = BiCalculator.GetBi(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    }
    
    /// <summary>
    /// 4. 获取线段
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("seglist")]
    public IActionResult SegList([FromBody] CalcRequest request)
    {
        var result = SegCalculator.GetSegs(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 5. 获取中枢高点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getsegpivotzg")]
    public IActionResult GetSegPivotZG([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetSegPivotZG(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 6. 获取中枢低点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getsegpivotzd")]
    public IActionResult GetSegPivotZD([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetSegPivotZD(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 7. 获取笔中枢起始点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getsegpivotrange")]
    public IActionResult GetSegPivotRange([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetSegPivotRange(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 8. 合并后的k线的高点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("klineg")]
    public IActionResult KLineG([FromBody] CalcRequest request)
    {
        var result = ChanKLineCalculator.GetKLineG(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 9. 合并后的k线的低点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("klined")]
    public IActionResult KLineD([FromBody] CalcRequest request)
    {
        var result = ChanKLineCalculator.GetKLineD(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 10. 合并后的k线的起始点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("klinerange")]
    public IActionResult KLineRange([FromBody] CalcRequest request)
    {
        var result = ChanKLineCalculator.GetKLineRange(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 


    /// <summary>
    /// 11. 设置MACD
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("setindicator")]
    public IActionResult SetIndicator([FromBody] CalcRequest request)
    {
        var result = IndicatorCalculator.Calculate(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    }
    
    /// <summary>
    /// 12. 获取中枢高点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getbipivotzg")]
    public IActionResult GetBiPivotZG([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetBiPivotZG(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 13. 获取中枢低点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getbipivotzd")]
    public IActionResult GetBiPivotZD([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetBiPivotZD(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 14. 获取笔中枢起始点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("getbipivotrange")]
    public IActionResult GetBiPivotRange([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetBiPivotRange(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    }
    
    /// <summary>
    /// 15. 设置成交量
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("setvolume")]
    public async Task<IActionResult> SetVolume([FromBody] CalcRequest request)
    {
        KLineDataPopulator.PopulateVolume(request.NCount, request.A, request.B, request.C);
        var key = request.C[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult?.UnitList == null)
            return Ok(new CalcResponse { Result = [] });

        var kLineDatas = KlineMapper.Map(calculateResult.UnitList, calculateResult.Symbol);

        // 使用 Task.Run 在线程池执行，避免同步上下文死锁
        //Task.Run(async () => await _setStockDataService.SaveKlinesAsync(kLineDatas));
        await _setStockDataService.SaveKlinesAsync(kLineDatas);

        return Ok(new CalcResponse { Result = [] });
    }

    /// <summary>
    /// 15. 将已有缠论计算结果转换为TradingView格式
    /// </summary>
    [HttpPost("tvdata")]
    public IActionResult GetTradingViewData([FromBody] CalcRequest request)
    {
        var key = request.C[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult == null)
        {
            return Ok(new TvChanlunResponse());
        }

        var response = ConvertToTvResponse(request.NCount, calculateResult);
        return Ok(response);
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

    /// <summary>
    /// 16. 一站式计算缠论并输出TradingView格式
    /// </summary>
    [HttpPost("tvchanlun")]
    public IActionResult CalculateTvChanlun([FromBody] TvChanlunRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol) || request.Bars == null || request.Bars.Count == 0)
            return BadRequest(new { error = "Symbol and Bars are required" });

        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var bars = request.Bars.OrderBy(b => b.Time).ToList();
        var n = bars.Count;

        try
        {
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
            //
            // ChanCalculator.Calculate(n, highs, lows, keyArray);
            //
            // var calculateResult = ChanCalculateResultCache.Get(key);
            // if (calculateResult == null)
            //     return StatusCode(500, new { error = "Chanlun calculation failed" });
            //
            // var response = ConvertToTvResponse(n, calculateResult);
            // response.Symbol = symbol;
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static TvChanlunResponse ConvertToTvResponse(int barCount, ChanCalculateResult result)
    {
        var response = new TvChanlunResponse
        {
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

public class CalcRequest
{
    public int NCount { get; set; }
    public decimal[] A { get; set; } = [];
    public decimal[] B { get; set; } = [];
    public decimal[] C { get; set; } = [];
}

public class CalcResponse
{
    public decimal[] Result { get; set; } = [];
}