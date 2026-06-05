using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

public record DividFactorsItem
{
    [JsonPropertyName("Date")]
    public required int Date { get; init; }
    
    [JsonPropertyName("Type")]
    public required int Type { get; init; }
    
    [JsonPropertyName("Bonus")]
    public required decimal Bonus { get; init; }
    
    [JsonPropertyName("AlloPrice")]
    public required decimal AlloPrice { get; init; }
    
    [JsonPropertyName("ShareBonus")]
    public required decimal ShareBonus { get; init; }
    
    [JsonPropertyName("Allotment")]
    public required decimal Allotment { get; init; }
}
