using Chanlun.Lib;
using Chanlun.Lib.Bi;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Memory;
using Chanlun.Lib.SEG;
using Chanlun.Lib.Zs;
using Microsoft.AspNetCore.Mvc;

namespace Chanlun.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculationController : ControllerBase
{
    /// <summary>
    /// 1. 生成key 已经记录所有的k 线时间
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("setstocktime")]
    public IActionResult SetStockTime([FromBody] CalcRequest request)
    {
        var result = StockTimeCache.Set(request.A, request.B, request.C);
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
    [HttpPost("pivotzg")]
    public IActionResult PivotZG([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetPivotZG(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 6. 获取中枢低点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("pivotzd")]
    public IActionResult PivotZD([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetPivotZD(request.NCount, request.A, request.B, request.C);
        return Ok(new CalcResponse { Result = result });
    } 
    
    /// <summary>
    /// 7. 获取笔中枢起始点
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("pivotrange")]
    public IActionResult PivotRange([FromBody] CalcRequest request)
    {
        var result = PivotCalculator.GetPivotRange(request.NCount, request.A, request.B, request.C);
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