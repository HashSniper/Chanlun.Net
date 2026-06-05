using System.Runtime.InteropServices;
using System.Text.Json;
using TdxQuantNet.Interfaces;
using TdxQuantNet.Models;

namespace TdxQuantNet;

public partial class TdxQuant : ITdxQuantOptional
{
    public SectorItem[] GetUserSector()
    {
        var ptr = GetUserBlockInStr(_runId, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetUserSector failed {error}");
        var result = response["Value"].Deserialize<SectorItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ GetUserSector failed {error}");
    }

    public bool SendUserBlock(string blockCode,  bool show = false, params string[] stocks)
    {
        var data = string.Empty;
        foreach (var stock in stocks)
        {
            if (!stock.Contains('.')) continue;
            var codeSplit = stock.Split(".", 1);
            var ex = codeSplit[^1] switch
            {
                "SZ" => "0#",
                "SH" => "1#",
                "BJ" => "2#",
                _ => null
            };
            if (ex is null) continue;
            data += $"{ex}{codeSplit[0]}|";
        }
        
        data = "XG," + blockCode + "||" + data + "|" + (show ? "1" : "0");
        
        var ptr = SetResToMain(_runId, _runMode, data, 6000);
        var str = Marshal.PtrToStringUTF8(ptr);
        if (str is null) throw new Exception("TQ SendUserBlock failed");
        var response = JsonSerializer.Deserialize<CallErrorModel>(str, Helper.JsonSerializerOptions);
        if (response is null) throw new Exception("TQ SendUserBlock failed");
        return response.ErrorId == 0;
    }

    public bool ClearSector(string blockCode)
    {
        var ptr = UserBlockControl(_runId, 4, blockCode, "none", 6000);
        var str = Marshal.PtrToStringUTF8(ptr);
        if (str is null) throw new Exception("TQ ClearSector failed");
        var response = JsonSerializer.Deserialize<CallErrorModel>(str, Helper.JsonSerializerOptions);
        if (response is null) throw new Exception("TQ ClearSector failed");
        return response.ErrorId == 0;
    }

    public bool CreateSector(string blockCode, string blockName)
    {
        var ptr = UserBlockControl(_runId, 1, blockCode, blockName, 6000);
        var str = Marshal.PtrToStringUTF8(ptr);
        if (str is null) throw new Exception("TQ CreateSector failed");
        var response = JsonSerializer.Deserialize<CallErrorModel>(str, Helper.JsonSerializerOptions);
        if (response is null) throw new Exception("TQ CreateSector failed");
        return response.ErrorId == 0;
    }

    public bool DeleteSector(string blockCode)
    {
        var ptr = UserBlockControl(_runId, 2, blockCode, "none", 6000);
        var str = Marshal.PtrToStringUTF8(ptr);
        if (str is null) throw new Exception("TQ DeleteSector failed");
        var response = JsonSerializer.Deserialize<CallErrorModel>(str, Helper.JsonSerializerOptions);
        if (response is null) throw new Exception("TQ DeleteSector failed");
        return response.ErrorId == 0;
    }

    public bool RenameSctor(string blockCode, string blockName)
    {
        var ptr = UserBlockControl(_runId, 3, blockCode, blockName, 6000);
        var str = Marshal.PtrToStringUTF8(ptr);
        if (str is null) throw new Exception("TQ RenameSctor failed");
        var response = JsonSerializer.Deserialize<CallErrorModel>(str, Helper.JsonSerializerOptions);
        if (response is null) throw new Exception("TQ RenameSctor failed");
        return response.ErrorId == 0;
    }
}
