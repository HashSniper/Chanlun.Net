using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record FormatKLine
{
    [JsonPropertyName("Date")]
    public required string Date { get; init; }
    
    [JsonPropertyName("Amount")]
    public required decimal Amount { get; init; }
    
    [JsonPropertyName("Volume")]
    public required decimal Volume { get; init; }
    
    [JsonPropertyName("Close")]
    public required decimal Close { get; init; }
    
    [JsonPropertyName("Open")]
    public required decimal Open { get; init; }
    
    [JsonPropertyName("High")]
    public required decimal High { get; init; }
    
    [JsonPropertyName("Low")]
    public required decimal Low { get; init; }
}
