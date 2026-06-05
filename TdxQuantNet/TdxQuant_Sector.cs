using System.Text.Json;
using TdxQuantNet.Interfaces;
using TdxQuantNet.Models;

namespace TdxQuantNet;


public partial class TdxQuant : ITdxQuantSector
{
    public SectorItem[] GetStockList(string market)
    {
        var ptr = GetStockListInStr(_runId, market, 1, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetStockList failed {error}");
        var result = response["Value"].Deserialize<SectorItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ GetStockList failed {error}");
    }

    public SectorItem[] GetSectorList()
    {
        var ptr = GetBlockListInStr(_runId, 1, 6000);
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetSectorList failed {error}");
        var result = response["Value"].Deserialize<SectorItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ GetSectorList failed {error}");
    }

    public SectorItem[] GetStockListInSector(string blockCode, int? blockType = null)
    {
        blockCode = blockType switch
        {
            1 => "BKCODE." + blockCode,
            2 => "QH." + blockCode,
            _ => blockCode
        };

        var ptr = GetBlockStocksInStr(_runId, blockCode, 1, 6000);
        
        var response = Helper.ParsePtrJson(ptr, out var error);
        if (response?["Value"] is null) throw new Exception($"TQ GetStockListInSector failed {error}");
        var result = response["Value"].Deserialize<SectorItem[]>(Helper.JsonSerializerOptions);
        return result ?? throw new Exception($"TQ GetStockListInSector failed {error}");
    }
}
