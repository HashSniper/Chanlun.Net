using System.Text.Json;
using TdxQuantNet;
using Chanlun.Lib;
using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.KLine;

var td = Utils.ConvertToDateTime(1260605, 1500);

try
{
    using var tq = TdxQuantFactory.Create("tq.net.test1", @"C:\new_tdx64");
    var res = tq.GetMarketData("688318.SH", 1);
    Console.WriteLine(res);
}
catch (Exception ex)
{
    Console.WriteLine($"TDX 调用失败: {ex.Message}");
}

// 加载并反序列化调试输出的 JSON 文件
var logDir = @"E:\Code\MyCode\ChanLun.Net\Chanlun.API\bin\Debug\net10.0\logs";
var tdxFile = Path.Combine(logDir, "chan_calculate_result_tdx.json");
var webFile = Path.Combine(logDir, "chan_calculate_result_web.json");

var tdxResult = DeserializeResult(tdxFile);
var webResult = DeserializeResult(webFile);

if (tdxResult != null)
{
    Console.WriteLine($"[TDX] Symbol={tdxResult.Symbol}, Units={tdxResult.UnitList?.Count ?? 0}");
}

if (webResult != null)
{
    Console.WriteLine($"[Web] Symbol={webResult.Symbol}, Units={webResult.UnitList?.Count ?? 0}");
}

CompareUnitLists(tdxResult?.UnitList, webResult?.UnitList);

static void CompareUnitLists(List<KLineUnit>? tdxUnits, List<KLineUnit>? webUnits)
{
    if (tdxUnits == null || webUnits == null)
    {
        Console.WriteLine("无法比较：其中一个 UnitList 为 null");
        return;
    }

    Console.WriteLine($"\n开始比较 UnitList：TDX={tdxUnits.Count}, Web={webUnits.Count}");

    if (tdxUnits.Count != webUnits.Count)
    {
        Console.WriteLine($"数量不一致：TDX={tdxUnits.Count}, Web={webUnits.Count}");
    }

    var diffCount = 0;
    var count = Math.Min(tdxUnits.Count, webUnits.Count);
    for (int i = 0; i < count; i++)
    {
        var tdx = tdxUnits[i];
        var web = webUnits[i];
        var diffs = new List<string>();

        if (tdx.Time != web.Time)
            diffs.Add($"Time[TDX={tdx.Time:yyyy-MM-dd HH:mm}, Web={web.Time:yyyy-MM-dd HH:mm}]");
        if (tdx.Open != web.Open)
            diffs.Add($"Open[TDX={tdx.Open}, Web={web.Open}]");
        if (tdx.Close != web.Close)
            diffs.Add($"Close[TDX={tdx.Close}, Web={web.Close}]");
        if (tdx.High != web.High)
            diffs.Add($"High[TDX={tdx.High}, Web={web.High}]");
        if (tdx.Low != web.Low)
            diffs.Add($"Low[TDX={tdx.Low}, Web={web.Low}]");

        if (diffs.Count > 0)
        {
            diffCount++;
            Console.WriteLine($"  Index {i}: {string.Join(", ", diffs)}");
        }
    }

    if (diffCount == 0)
    {
        Console.WriteLine("所有对比项完全一致");
    }
    else
    {
        Console.WriteLine($"共有 {diffCount} 条记录存在差异");
    }
}

static ChanCalculateResultStub? DeserializeResult(string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"文件不存在: {filePath}");
        return null;
    }

    var json = File.ReadAllText(filePath);
    return JsonSerializer.Deserialize<ChanCalculateResultStub>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
}

/// <summary>
/// 仅用于反序列化所需字段（Symbol 与 UnitList）的轻量实体。
/// 由于 Seg/Eigen 等类型包含主构造函数的参数无法直接绑定到 JSON 属性，
/// 完整反序列化 ChanCalculateResult 需要额外调整领域模型；本处仅关注 UnitList 对比。
/// </summary>
public class ChanCalculateResultStub
{
    public string Symbol { get; set; } = string.Empty;
    public List<KLineUnit> UnitList { get; set; } = new();
}
