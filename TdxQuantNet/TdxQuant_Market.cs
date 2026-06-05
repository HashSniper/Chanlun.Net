using System.Text.Json;
using System.Text.Json.Nodes;
using TdxQuantNet.Interfaces;
using TdxQuantNet.Models;

namespace TdxQuantNet;

public partial class TdxQuant : ITdxQuantMarket
{
    public MarketDataModel GetMarketData(
        string code,
        string startDate,
        string? period = null,
        string? endDate = null,
        string? dividendType = null)
    {
        var dType = dividendType switch
        {
            "none" => 10,
            "front" => 1,
            "back" => 2,
            _ => 1
        };
        
        var start = Helper.ConvertTimeFormat(startDate);
        var end = Helper.ConvertTimeFormat(endDate ?? Helper.GetNowTimeFormat());
        
        var ptr = GetHisdaTsInStr(_runId, code, start, end, period ?? "1m", dType, -1, 6000);

        var response = Helper.ParsePtrJson<MarketDataModel>(ptr, out var error);
        return response ?? throw new Exception($"TQ GetMarketData failed {error}");
    }

    public MarketDataModel GetMarketData(
        string code,
        int count,
        string? period = null,
        string? endDate = null,
        string? dividendType = null)
    {
        if (count <= 0) throw new ArgumentException("count must be greater than 0");

        var dType = dividendType switch
        {
            "none" => 10,
            "front" => 1,
            "back" => 2,
            _ => 1
        };

        var end = Helper.ConvertTimeFormat(endDate ?? Helper.GetNowTimeFormat());

        var ptr = GetHisdaTsInStr(_runId, code, "", end, period ?? "1d", dType, count, 6000);

        var response = Helper.ParsePtrJson<MarketDataModel>(ptr, out var error);
        return response ?? throw new Exception($"TQ GetMarketData failed {error}");
    }

    public MarketSnapshotModel GetMarketSnapshot(string code)
    {
        var ptr = GetReportInStr(_runId, code, 6000);
        var response = Helper.ParsePtrJson<MarketSnapshotModel>(ptr, out var error);
        return response ?? throw new Exception($"TQ GetMarketSnapshot failed {error}");
    }

    public JsonNode GetStockInfo(string code)
    {
        var ptr = GetStockInStr(_runId, code, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response ?? throw new Exception($"TQ GetStockInfo failed {error}");
    }

    public JsonNode GetMoreInfo(string code)
    {
        var ptr = GetMoreInfoInStr(_runId, code, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetStockInfo failed {error}");
    }

    public DividFactorsItem[] GetDividFactors(string code, string startDate, string? endDate = null)
    {
        var start = Helper.ConvertTimeFormat(startDate);
        var end = Helper.ConvertTimeFormat(endDate ?? Helper.GetNowTimeFormat());
        var ptr = GetCwDataInStr(_runId, code, start, end, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response is null) throw new Exception($"TQ GetDividFactors failed {error}");

        var typeArr = response["Type"]!.Deserialize<int[]>(Helper.JsonSerializerOptions)!;
        var dateArr = response["Date"]!.Deserialize<int[]>(Helper.JsonSerializerOptions)!;
        var valueArr = response["Value"]!.Deserialize<decimal[][]>(Helper.JsonSerializerOptions)!;

        var result = new DividFactorsItem[typeArr.Length];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = new DividFactorsItem
            {
                Date = dateArr[i],
                Type = typeArr[i],
                Bonus = valueArr[i][0],
                AlloPrice = valueArr[i][1],
                ShareBonus = valueArr[i][2],
                Allotment = valueArr[i][3]
            };
        }

        return result;
    }

    public RelationItem[] GetRelation(string code)
    {
        var ptr = GetGpBlockInStr(_runId, code, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetRelation failed {error}");
        var result = response["Value"].Deserialize<RelationItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ GetRelation failed {error}");
    }

    public IpoInfoItem[] GetIpoInfo(int ipoType, int ipoDate)
    {
        var ptr = GetIpoInfoInStr(_runId, ipoType, ipoDate, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetIpoInfo failed {error}");
        var result = response["Value"].Deserialize<IpoInfoItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ GetIpoInfo failed {error}");
    }

    public GbInfoItem[] GetGbInfo(string code, params string[] dateList)
    {
        var json = JsonSerializer.Serialize(new
        {
            stock_code = code,
            date_list = dateList,
            count = dateList.Length
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetGbInfoInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetGbInfo failed {error}");
        var result = response["Value"].Deserialize<GbInfoItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ GetGbInfo failed {error}");
    }
}
