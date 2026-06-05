using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record StockPositionsItem
{
    [JsonPropertyName("Code")] 
    public required string Code { get; init; }
    
    [JsonPropertyName("Cbj")]
    public required decimal Cbj { get; init; }
    
    [JsonPropertyName("TotalVol")] 
    public required int TotalVol { get; init; }
    
    [JsonPropertyName("CanUseVol")] 
    public required int CanUseVol { get; init; }
}
