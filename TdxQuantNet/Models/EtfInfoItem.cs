using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record EtfInfoItem
{
    [JsonPropertyName("Code")] 
    public required string Code { get; init; }
    
    [JsonPropertyName("Name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("NowPrice")]
    public required decimal NowPrice { get; init; }
    
    [JsonPropertyName("PreClose")]
    public required decimal PreClose { get; init; }
    
    [JsonPropertyName("IOPV")]
    public required decimal Iopv { get; init; }
    
    [JsonPropertyName("Zgb")]
    public required decimal Zgb { get; init; }
    
    [JsonPropertyName("Sz")]
    public required decimal Sz { get; init; }
}
