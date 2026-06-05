using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record GbInfoItem
{
    [JsonPropertyName("Date")]
    public required int Date { get; init; }
    
    [JsonPropertyName("Zgb")]
    public required decimal Zgb { get; init; }
    
    [JsonPropertyName("Ltgb")]
    public required decimal Ltgb { get; init; }
}
