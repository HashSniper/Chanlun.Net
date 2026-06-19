using System.Text.Json;
using System.Text.Json.Serialization;
using Chanlun.Lib;
using Chanlun.Lib.Bi;
using Chanlun.Lib.ChanCommon;
using Chanlun.Lib.Extensions;
using Chanlun.Lib.KLine;
using Chanlun.Lib.Memory;
using Chanlun.Lib.SEG;
using Chanlun.Lib.StockIndicators;
using Chanlun.Lib.TradingPoint;
using Chanlun.Lib.Zs;

namespace Chanlun.API.Adapter;

public static class ChanCalculator
{
    public static ChanCalculateResult Calculate(decimal[] pkey)
    {
        decimal key = pkey[0];
        var result = ChanCalculateResultCache.Get(key);

        ChanKLineCalculator.Calculate(ref result);
        BiCalculator.Calculate(ref result);
        SegCalculator.Calculate(ref result);
        PivotCalculator.Calculate(ref result);
        ChanCalculateResultCache.Add(key, result);

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
            var filePath = Path.Combine(logDir, $"chan_calculate_result_tdx.json");
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

public static class KLineDataPopulator
{
    private static decimal BuildBaseKey(string code, decimal[] date, decimal[] time)
    {
        var keyString = $"{code}_{date[0]}:{time[0]}-{date[^1]}:{time[^1]}_{date.Length}";
        // FNV-1a 哈希，取低24位确保在 decimal 精确整数范围内（2^24 = 16,777,216）
        uint hash = 2166136261u;
        foreach (char c in keyString)
        {
            hash ^= c;
            hash *= 16777619u;
        }
        return hash & 0x00FFFFFF;
    }
    
    /// <summary>
    /// 必须在第一步执行
    /// </summary>
    /// <param name="nCount"></param>
    /// <param name="code"></param>
    /// <param name="date"></param>
    /// <param name="time"></param>
    public static decimal[] PopulateTradeTime(int nCount, decimal[] code, decimal[] date, decimal[] time)
    {
        int codeValue = (int)code[0];
        string stockCode = codeValue.ToString("D6");
        
        var baseKey = BuildBaseKey(stockCode, date, time);
        var result = new ChanCalculateResult
        {
            Symbol = stockCode
        };
        var unitList = new List<KLineUnit>();
        for (int i = 0; i < date.Length; i++)
        {
            int d = (int)date[i];
            int t = (int)time[i];
            
            unitList.Add(new KLineUnit(i)
            {
                Time = Utils.ConvertToDateTime(d, t)
            });
        }

        result.UnitList = unitList;
        
        var pOut = new decimal[date.Length];
        pOut[0] = baseKey;
        for (int i = 1; i < nCount; ++i)
        {
            pOut[i] = i;
        }
        ChanCalculateResultCache.Add(baseKey, result);
        
        return pOut;
    }

    public static void PopulateHighLowPrice(int nCount, decimal[] pHigh, decimal[] pLow, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult?.UnitList == null)
        {
            return;
        }

        for (int i = 0; i < nCount; ++i)
        {
            var kLineUnit = calculateResult.UnitList[i];
            kLineUnit.High = pHigh[i];
            kLineUnit.Low = pLow[i];
        }
    }

    /// <summary>
    /// 处理k 线的最后一步
    /// </summary>
    /// <param name="nCount"></param>
    /// <param name="pVolume"></param>
    /// <param name="pAmount"></param>
    /// <param name="pKey"></param>
    public static void PopulateVolume(int nCount, decimal[] pVolume, decimal[] pAmount, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult?.UnitList == null)
        {
            return;
        }

        for (int i = 0; i < nCount; ++i)
        {
            var kLineUnit = calculateResult.UnitList[i];
            kLineUnit.Amount = pAmount[i];
            kLineUnit.Volume = pVolume[i];
        }

    }
    
    public static void PopulateOpenClosePrice(int nCount, decimal[] pOpen, decimal[] pClose, decimal[] pKey)
    {
        var key = pKey[0];
        var calculateResult = ChanCalculateResultCache.Get(key);
        if (calculateResult?.UnitList == null)
        {
            return;
        }

        for (int i = 0; i < nCount; ++i)
        {
            var kLineUnit = calculateResult.UnitList[i];
            kLineUnit.Open = pOpen[i];
            kLineUnit.Close = pClose[i];
        }
    }
}
