using Microsoft.AspNetCore.Mvc;

namespace Chanlun.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculationController : ControllerBase
{
    [HttpPost("bi1")]
    public IActionResult Bi1([FromBody] BiRequest request)
    {
        var result = BiCalculator.Bi1(request.NCount, request.High, request.Low);
        return Ok(new CalcResponse { Result = result });
    }

    [HttpPost("bi2")]
    public IActionResult Bi2([FromBody] BiRequest request)
    {
        var result = BiCalculator.Bi2(request.NCount, request.High, request.Low);
        return Ok(new CalcResponse { Result = result });
    }

    [HttpPost("duan1")]
    public IActionResult Duan1([FromBody] DuanRequest request)
    {
        var result = DuanCalculator.Duan1(request.NCount, request.Bi, request.High, request.Low);
        return Ok(new CalcResponse { Result = result });
    }

    [HttpPost("duan2")]
    public IActionResult Duan2([FromBody] DuanRequest request)
    {
        var result = DuanCalculator.Duan2(request.NCount, request.Bi, request.High, request.Low);
        return Ok(new CalcResponse { Result = result });
    }

    [HttpPost("zs/high")]
    public IActionResult ZsHigh([FromBody] ZsRequest request)
    {
        var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.Bi, request.High, request.Low);
        float[] pOut = new float[request.NCount];
        foreach (var zs in zhongShuList)
        {
            for (int j = zs.S + 1; j <= zs.E - 1; j++)
                pOut[j] = zs.Zg;
        }
        return Ok(new CalcResponse { Result = pOut });
    }

    [HttpPost("zs/low")]
    public IActionResult ZsLow([FromBody] ZsRequest request)
    {
        var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.Bi, request.High, request.Low);
        float[] pOut = new float[request.NCount];
        foreach (var zs in zhongShuList)
        {
            for (int j = zs.S + 1; j <= zs.E - 1; j++)
                pOut[j] = zs.Zd;
        }
        return Ok(new CalcResponse { Result = pOut });
    }

    [HttpPost("zs/signal")]
    public IActionResult ZsSignal([FromBody] ZsRequest request)
    {
        var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.Bi, request.High, request.Low);
        float[] pOut = new float[request.NCount];
        foreach (var zs in zhongShuList)
        {
            pOut[zs.S + 1] = 1;
            pOut[zs.E - 1] = 2;
        }
        return Ok(new CalcResponse { Result = pOut });
    }

    [HttpPost("zs/direction")]
    public IActionResult ZsDirection([FromBody] ZsRequest request)
    {
        var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.Bi, request.High, request.Low);
        float[] pOut = new float[request.NCount];
        foreach (var zs in zhongShuList)
        {
            for (int j = zs.S + 1; j <= zs.E - 1; j++)
                pOut[j] = zs.Direction;
        }
        return Ok(new CalcResponse { Result = pOut });
    }

    [HttpPost("zs/index")]
    public IActionResult ZsIndex([FromBody] ZsRequest request)
    {
        var zhongShuList = ZhongShuCalculator.ZS(request.NCount, request.Bi, request.High, request.Low);
        float[] pOut = new float[request.NCount];
        for (int i = 0; i < zhongShuList.Count; i++)
        {
            var zs = zhongShuList[i];
            float c = 1;
            for (int j = i - 1; j >= 0; j--)
            {
                if (zhongShuList[j].Direction == zs.Direction)
                    c++;
                else
                    break;
            }
            for (int j = zs.S + 1; j <= zs.E - 1; j++)
                pOut[j] = c;
        }
        return Ok(new CalcResponse { Result = pOut });
    }
}

public class BiRequest
{
    public int NCount { get; set; }
    public float[] High { get; set; } = [];
    public float[] Low { get; set; } = [];
}

public class DuanRequest
{
    public int NCount { get; set; }
    public float[] Bi { get; set; } = [];
    public float[] High { get; set; } = [];
    public float[] Low { get; set; } = [];
}

public class ZsRequest
{
    public int NCount { get; set; }
    public float[] Bi { get; set; } = [];
    public float[] High { get; set; } = [];
    public float[] Low { get; set; } = [];
}

public class CalcResponse
{
    public float[] Result { get; set; } = [];
}
