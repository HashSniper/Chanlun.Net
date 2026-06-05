using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global

namespace TdxQuantNet.Models;

public record OrderStockModel
{
    [JsonPropertyName("Data")] public required int Data { get; init; }
    
    [JsonPropertyName("ID")] public required int Id { get; init; }
    
    [JsonPropertyName("Msg")] public required string Msg { get; init; }
}
