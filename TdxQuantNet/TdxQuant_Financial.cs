using System.Text.Json;
using System.Text.Json.Nodes;
using TdxQuantNet.Interfaces;

namespace TdxQuantNet;

public partial class TdxQuant : ITdxQuantFinancial
{
    public JsonNode GetFinancialData(string code, string start, string end, string reportType = "announce_time",
        params string[] fieldList)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "1",
            code,
            table_list = fieldList,
            start_time = Helper.ConvertTimeFormat(start),
            end_time = Helper.ConvertTimeFormat(end),
            report_type = reportType
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetProDataInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetFinancialData failed {error}");
    }

    public JsonNode GetFinancialDataByDate(string code, int year, int mmdd, params string[] fieldList)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "2",
            code,
            table_list = fieldList,
            year,
            mmdd
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetProDataInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetFinancialDataByDate failed {error}");
    }

    public JsonNode GetGpJyValueByDate(string code, int year, int mmdd, params string[] fieldList)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "4",
            code,
            table_list = fieldList,
            year,
            mmdd
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetProDataInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetGpJyValueByDate failed {error}");
    }

    public JsonNode GetBkJyValue(string code, string start, string end, params string[] fieldList)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "5",
            code,
            table_list = fieldList,
            start_time = Helper.ConvertTimeFormat(start),
            end_time = Helper.ConvertTimeFormat(end),
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetProDataInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetBkJyValue failed {error}");
    }

    public JsonNode GetBkJyValueByDate(string code, int year, int mmdd, params string[] fieldList)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "6",
            code,
            table_list = fieldList,
            year,
            mmdd
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetProDataInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetBkJyValueByDate failed {error}");
    }

    public JsonNode GetScJyValue(string start, string end, params string[] fieldList)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "7",
            code = "999999.SH",
            table_list = fieldList,
            start_time = Helper.ConvertTimeFormat(start),
            end_time = Helper.ConvertTimeFormat(end),
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetProDataInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetScJyValue failed {error}");
    }
    
    public JsonNode GetScJyValueByDate(int year, int mmdd, params string[] fieldList)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "8",
            code = "999999.SH",
            table_list = fieldList,
            year,
            mmdd
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetProDataInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetScJyValueByDate failed {error}");
    }

    public JsonNode GetGpOneData(string code, params string[] fieldList)
    {
        var json = JsonSerializer.Serialize(new
        {
            id = _runId,
            type = "9",
            code,
            table_list = fieldList,
        }, Helper.InputJsonSerializerOptions);
        var ptr = GetProDataInStr(_runId, json, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        return response?["Value"] ?? throw new Exception($"TQ GetGpOneData failed {error}");
    }
}
