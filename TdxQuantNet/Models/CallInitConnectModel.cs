using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
internal record CallInitConnectModel
{
    [JsonPropertyName("Error")]
    public required string Error { get; init; }
    
    [JsonPropertyName("ErrorId")]
    public required int ErrorId { get; init; }

    [JsonPropertyName("run_id")] 
    public required int RunId { get; init; }
}
