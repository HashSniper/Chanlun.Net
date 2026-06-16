using Chanlun.API.Adapter;
using Stock.Service.Adapter;
using Chanlun.API.Models;
using Chanlun.Lib.Bi;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Memory;
using Chanlun.Lib.SEG;
using Chanlun.Lib.StockIndicators;
using Chanlun.Lib.Zs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Chanlun.API.Hubs;
using Stock.Data.Entities;
using Stock.Service.Interface;

namespace Chanlun.API.Controllers;

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

[ApiController]
[Route("api/calculation")]
public class TdxCalculationController : ControllerBase
{
    private readonly ISetKlineService _setKlineService;
    private readonly ISaveTdxCurrentKlineViewService _recordService;
    private readonly ILogger<TdxCalculationController> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<ChanlunHub> _hubContext;

    public TdxCalculationController(ISetKlineService setKlineService, ISaveTdxCurrentKlineViewService recordService, ILogger<TdxCalculationController> logger, IServiceScopeFactory serviceScopeFactory, IHubContext<ChanlunHub> hubContext)
    {
        _setKlineService = setKlineService;
        _recordService = recordService;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
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
    public async Task<IActionResult> CreateChan([FromBody] CalcRequest request)
    {
        KLineDataPopulator.PopulateHighLowPrice(request.NCount, request.A, request.B, request.C);
        var calResult = ChanCalculator.Calculate(request.C);

        TdxCurrentKlineView currentView = new TdxCurrentKlineView();
        currentView.Symbol = calResult.Symbol;
        currentView.StartTime = calResult.UnitList[0].Time;
        currentView.EndTime = calResult.UnitList[^1].Time;
        currentView.Resolution = KlineMapper.DetectResolution(calResult.UnitList);

        // 使用 Task.Run 在线程池中异步保存数据，避免阻塞 API 响应
        // 在后台任务内创建新的 DI Scope，避免请求结束后 DbContext 被释放
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var recordService = scope.ServiceProvider.GetRequiredService<ISaveTdxCurrentKlineViewService>();
                await recordService.SaveTdxCurrentKlineView(currentView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save TdxCurrentKlineView");
            }
        });

        return Ok(new CalcResponse { Result = [] });
    }

    /// <summary>
    /// 15. 设置成交量
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("setvolume")]
    public IActionResult SetVolume([FromBody] CalcRequest request)
    {
        KLineDataPopulator.PopulateVolume(request.NCount, request.A, request.B, request.C);
        var key = request.C[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult?.UnitList == null)
            return Ok(new CalcResponse { Result = [] });

        var kLineDatas = KlineMapper.Map(calculateResult.UnitList, calculateResult.Symbol);

        // 使用 Task.Run 在线程池中异步保存数据，避免阻塞 API 响应
        // 在后台任务内创建新的 DI Scope，避免请求结束后 DbContext 被释放
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var setKlineService = scope.ServiceProvider.GetRequiredService<ISetKlineService>();
                await setKlineService.SaveKlinesAsync(kLineDatas);

                // 保存完成后通知所有前端客户端刷新通达信当前窗口数据
                _logger.LogInformation("Klines saved, sending TdxDataUpdated notification to all SignalR clients");
                await _hubContext.Clients.All.SendAsync(ChanlunHub.TdxDataUpdatedMethod);
                _logger.LogInformation("TdxDataUpdated notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save klines");
            }
        });

        return Ok(new CalcResponse { Result = [] });
    }

    /// <summary>
    /// 11. 设置MACD
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("setindicator")]
    public IActionResult SetIndicator([FromBody] CalcRequest request)
    {
        KLineDataPopulator.PopulateOpenClosePrice(request.NCount, request.A, request.B, request.C);
        var key = request.C[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        var result = IndicatorCalculator.Calculate(ref calculateResult);
        return Ok(new CalcResponse { Result = result });
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

}
