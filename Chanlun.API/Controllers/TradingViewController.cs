using Chanlun.API.Adapter;
using Chanlun.Lib.Adapter;
using Microsoft.AspNetCore.Mvc;
using Stock.Data.Entities;
using Stock.Service.Interface;

namespace Chanlun.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingViewController : ControllerBase
{
    private readonly IGetKLineService _getKLineService;
    private readonly IGetTdxCurrentKlineViewService _recordService;

    public TradingViewController(IGetKLineService getKLineService, IGetTdxCurrentKlineViewService recordService)
    {
        _getKLineService = getKLineService;
        _recordService = recordService;
    }

    [HttpGet("chanlunklines")]
    public async Task<IActionResult> ChanLunKLines(
        [FromQuery] string symbol,
        [FromQuery] string resolution,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var resolutionEnum = Enum.Parse<KlineResolution>(resolution);
        var kLines = await _getKLineService.GetKlinesAsync(new GetKLineQuery()
        {
            Symbol = symbol,
            Resolution = resolutionEnum,
            FromTime = from,
            ToTime = to
        });

        var chanResult = ChanCalculateResultBuilder.Build(symbol, kLines.ToKLineUnits());
        var response =
            ChanlunResultAdapter.ConvertToTvResponse(chanResult.UnitList?.Count ?? 0, chanResult, resolution);
        return Ok(response);
    }
    
    [HttpGet("tdxchanlunklines")]
    public async Task<IActionResult> TdxChanLunKLines()
    {
        var currentStock = await _recordService.GetTdxCurrentKlineView();
        if (currentStock == null)
        {
            return NotFound();
        }

        var kLines = await _getKLineService.GetKlinesAsync(new GetKLineQuery()
        {
            Symbol = currentStock.Symbol,
            Resolution = currentStock.Resolution,
            FromTime = currentStock.StartTime,
            ToTime = currentStock.EndTime
        });
    
        var chanResult = ChanCalculateResultBuilder.Build(currentStock.Symbol, kLines.ToKLineUnits());
        var response = ChanlunResultAdapter.ConvertToTvResponse(chanResult.UnitList?.Count ?? 0, chanResult,
            currentStock.Resolution.ToString());
        return Ok(response);
    }
}
