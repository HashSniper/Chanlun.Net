using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record SectorItem
{
    [JsonPropertyName("Code")]
    public required string Code { get; init;}
    
    [JsonPropertyName("Name")]
    public required string Name { get; init;}
}
