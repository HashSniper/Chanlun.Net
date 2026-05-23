using Chanlun.Lib.Bi;
using Chanlun.Lib.Memory;
using Microsoft.AspNetCore.Mvc;

namespace Chanlun.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculationController : ControllerBase
{
    [HttpPost("setstocktime")]
    public IActionResult SetStockTime([FromBody] CalcRequest request)
    {
        
        var result = StockTimeCache.Set(request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    }

    [HttpPost("createbi")]
    public IActionResult CreateBi([FromBody] CalcRequest request)
    {
        var result = BiCalculator.Bi(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    }
    
    [HttpPost("createbizg")]
    public IActionResult CreateBiZG([FromBody] CalcRequest request)
    {
        var result = BiCalculator.GetKLineCombineIndex(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    [HttpPost("stockindex")]
    public IActionResult StockIndex([FromBody] CalcRequest request)
    {
        var result = BiCalculator.GetKLineCombineIndex(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 

    [HttpPost("duan1")]
    public IActionResult Duan1([FromBody] CalcRequest request)
    {
        //var result = DuanCalculator.Duan1(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = [] });
    }

    [HttpPost("duan2")]
    public IActionResult Duan2([FromBody] CalcRequest request)
    {
        //var result = DuanCalculator.Duan2(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = [] });
    }

    [HttpPost("zs/high")]
    public IActionResult ZsHigh([FromBody] CalcRequest request)
    {
        // var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.A, request.B, request.C);
        // float[] pOut = new float[request.NCount];
        // foreach (var zs in zhongShuList)
        // {
        //     for (int j = zs.S + 1; j <= zs.E - 1; j++)
        //         pOut[j] = zs.Zg;
        // }
        return Ok(new CalcResponse { Result = [] });
    }

    [HttpPost("zs/low")]
    public IActionResult ZsLow([FromBody] CalcRequest request)
    {
        // var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.A, request.B, request.C);
        // float[] pOut = new float[request.NCount];
        // foreach (var zs in zhongShuList)
        // {
        //     for (int j = zs.S + 1; j <= zs.E - 1; j++)
        //         pOut[j] = zs.Zd;
        // }
        return Ok(new CalcResponse { Result = [] });
    }

    [HttpPost("zs/signal")]
    public IActionResult ZsSignal([FromBody] CalcRequest request)
    {
        // var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.A, request.B, request.C);
        // float[] pOut = new float[request.NCount];
        // foreach (var zs in zhongShuList)
        // {
        //     pOut[zs.S + 1] = 1;
        //     pOut[zs.E - 1] = 2;
        // }
        return Ok(new CalcResponse { Result = [] });
    }

    [HttpPost("zs/direction")]
    public IActionResult ZsDirection([FromBody] CalcRequest request)
    {
        // var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.A, request.B, request.C);
        // float[] pOut = new float[request.NCount];
        // foreach (var zs in zhongShuList)
        // {
        //     for (int j = zs.S + 1; j <= zs.E - 1; j++)
        //         pOut[j] = zs.Direction;
        // }
        return Ok(new CalcResponse { Result = [] });
    }

    [HttpPost("zs/index")]
    public IActionResult ZsIndex([FromBody] CalcRequest request)
    {
        // var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.A, request.B, request.C);
        // float[] pOut = new float[request.NCount];
        // for (int i = 0; i < zhongShuList.Count; i++)
        // {
        //     var zs = zhongShuList[i];
        //     float c = 1;
        //     for (int j = i - 1; j >= 0; j--)
        //     {
        //         if (zhongShuList[j].Direction == zs.Direction)
        //             c++;
        //         else
        //             break;
        //     }
        //     for (int j = zs.S + 1; j <= zs.E - 1; j++)
        //         pOut[j] = c;
        // }
        return Ok(new CalcResponse { Result = [] });
    }
}

public class CalcRequest
{
    public int NCount { get; set; }
    public float[] A { get; set; } = [];
    public float[] B { get; set; } = [];
    public float[] C { get; set; } = [];
}

public class CalcResponse
{
    public float[] Result { get; set; } = [];
}
