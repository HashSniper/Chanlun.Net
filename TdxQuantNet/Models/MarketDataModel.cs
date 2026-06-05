using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record MarketDataModel
{
    [JsonPropertyName("Date")] 
    public required int[] DateFrame { get; init; }
    
    [JsonPropertyName("Time")] 
    public required int[] TimeFrame { get; init; }

    [JsonPropertyName("Open")] 
    public required float[] OpenFrame { get; init; }

    [JsonPropertyName("High")] 
    public required float[] HighFrame { get; init; }

    [JsonPropertyName("Low")] 
    public required float[] LowFrame { get; init; }

    [JsonPropertyName("Close")]
    public required float[] CloseFrame { get; init; }

    [JsonPropertyName("Volume")]
    public required decimal[] VolumeFrame { get; init; }

    [JsonPropertyName("Amount")]
    public required decimal[] AmountFrame { get; init; }
}
