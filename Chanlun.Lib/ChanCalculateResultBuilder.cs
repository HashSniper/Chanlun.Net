using System.Text.Json;
using System.Text.Json.Serialization;
using Chanlun.Lib.Bi;
using Chanlun.Lib.KLine;
using Chanlun.Lib.SEG;
using Chanlun.Lib.StockIndicators;
using Chanlun.Lib.TradingPoint;
using Chanlun.Lib.Zs;

namespace Chanlun.Lib;

public static class ChanCalculateResultBuilder
{
    public static ChanCalculateResult Build(string symbol, List<KLineUnit> units)
    {
        var result = new ChanCalculateResult()
        {
            Symbol = symbol,
            UnitList = units
        };
        ChanKLineCalculator.Calculate(ref result);
        BiCalculator.Calculate(ref result);
        SegCalculator.Calculate(ref result);
        PivotCalculator.Calculate(ref result); 
        IndicatorCalculator.Calculate(ref result);
        ChanTradingPointCalculator.Calculate(ref result);
        
#if DEBUG
        try
        {
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            });

            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            var filePath = Path.Combine(logDir, $"chan_calculate_result_web.json");
            File.WriteAllText(filePath, json);
        }
        catch
        {
            // 调试输出不影响主流程
        }
#endif

        
        return result;
    }
}