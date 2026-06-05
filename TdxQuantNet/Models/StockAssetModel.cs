using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record StockAssetModel
{
    [JsonPropertyName("Currency")]
    public required string Currency { get; init; }
    
    [JsonPropertyName("Balance")]
    public required decimal Balance { get; init; }
    
    [JsonPropertyName("Cash")]
    public required decimal Cash { get; init; }
    
    [JsonPropertyName("Asset")]
    public required decimal Asset { get; init; }
    
    [JsonPropertyName("MarketValue")]
    public required decimal MarketValue { get; init; }
}
