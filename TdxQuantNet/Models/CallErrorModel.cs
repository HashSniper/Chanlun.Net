using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

internal record CallErrorModel
{
    [JsonPropertyName("Error")]
    public string? Error { get; init; }
    
    [JsonPropertyName("ErrorId")]
    public required int ErrorId { get; init; }

    [JsonPropertyName("run_id")] 
    public int RunId { get; init; } = -1;
}
