using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record RelationItem
{
    [JsonPropertyName("BlockCode")] public required string BlockCode { get; init; }
    [JsonPropertyName("BlockName")] public required string BlockName { get; init; }
    [JsonPropertyName("BlockType")] public required string BlockType { get; init; }
    [JsonPropertyName("GPNume")] public required decimal GpNume { get; init; }
}
