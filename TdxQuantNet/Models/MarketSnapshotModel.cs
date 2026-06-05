using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;


// ReSharper disable once ClassNeverInstantiated.Global
public record MarketSnapshotModel
{
    [JsonPropertyName("ItemNum")]
    public required int ItemNum { get; init; }
    
    [JsonPropertyName("LastClose")]
    public required decimal LastClose { get; init; }
    
    [JsonPropertyName("Open")]
    public required decimal Open { get; init; }
    
    [JsonPropertyName("Max")]
    public required decimal Max { get; init; }
    
    [JsonPropertyName("Min")]
    public required decimal Min { get; init; }
    
    [JsonPropertyName("Now")]
    public required decimal Now { get; init; }
    
    [JsonPropertyName("Volume")]
    public required decimal Volume { get; init; }
    
    [JsonPropertyName("NowVol")]
    public required decimal NowVol { get; init; }
    
    [JsonPropertyName("Amount")]
    public required decimal Amount { get; init; }
    
    [JsonPropertyName("Inside")]
    public required int Inside { get; init; }
    
    [JsonPropertyName("Outside")]
    public required int Outside { get; init; }
    
    [JsonPropertyName("TickDiff")]
    public required decimal TickDiff { get; init; }
    
    [JsonPropertyName("InOutFlag")]
    public required int InOutFlag { get; init; }
    
    [JsonPropertyName("Jjjz")]
    public required decimal Jjjz { get; init; }
    
    [JsonPropertyName("Buyp")]
    public required decimal[] Buyp { get; init; }
    
    [JsonPropertyName("Buyv")]
    public required decimal[] Buyv { get; init; }
    
    [JsonPropertyName("Sellp")]
    public required decimal[] Sellp { get; init; }
    
    [JsonPropertyName("Sellv")]
    public required decimal[] Sellv { get; init; }
    
    [JsonPropertyName("UpHome")]
    public required int UpHome { get; init; }
    
    [JsonPropertyName("DownHome")]
    public required int DownHome { get; init; }
    
    [JsonPropertyName("Before5MinNow")]
    public required decimal Before5MinNow { get; init; }
    
    [JsonPropertyName("Average")]
    public required decimal Average { get; init; }
    
    [JsonPropertyName("XsFlag")]
    public required int XsFlag { get; init; }
    
    [JsonPropertyName("Zangsu")]
    public required decimal Zangsu { get; init; }
    
    [JsonPropertyName("ZAFPre3")]
    public required decimal ZafPre3 { get; init; }
}
