using System.Text.Json;
using TdxQuantNet.Interfaces;
using TdxQuantNet.Models;

namespace TdxQuantNet;

public partial class TdxQuant : ITdxQuantDerived
{
    public EtfInfoItem[] GetTrackzsEtfInfo(string zsCode)
    {
        var ptr = GetTrackZsEtfInfoInStr(_runId, zsCode, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetTrackzsEtfInfo failed {error}");
        var result = response["Value"].Deserialize<EtfInfoItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ GetTrackzsEtfInfo failed {error}");
    }

    public KzzInfoItem GetKzzInfo(string stockCode)
    {
        var ptr = GetCbInfoInStr(_runId, stockCode, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetKzzInfo failed {error}");
        var result = response["Value"].Deserialize<KzzInfoItem[]>(Helper.JsonSerializerOptions);
        if (result is null || result.Length == 0) throw new Exception($"TQ GetKzzInfo failed {error}"); 
        return result[0];
    }
}
