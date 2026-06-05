using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record StockOrderItem
{
    [JsonPropertyName("Wtbh")]
    public required string Wtbh { get; init; }
    
    [JsonPropertyName("Code")]
    public required string Code { get; init; }
    
    [JsonPropertyName("Time")]
    public required string Time { get; init; }
    
    [JsonPropertyName("BSFlag")]
    public required int BsFlag { get; init; }
    
    [JsonPropertyName("KPFlag")]
    public required int KpFlag { get; init; }
    
    [JsonPropertyName("WTFS")]
    public required string Wtfs { get; init; }
    
    [JsonPropertyName("Status")]
    public required int Status { get; init; }
    
    [JsonPropertyName("WtDate")]
    public required int WtDate { get; init; }
    
    [JsonPropertyName("CjPric")]
    public required decimal CjPric { get; init; }
    
    [JsonPropertyName("CJVol")]
    public required decimal CjVol { get; init; }
    
    [JsonPropertyName("WtPrice")]
    public required decimal WtPrice { get; init; }
    
    [JsonPropertyName("WtVol")]
    public required decimal WtVol { get; init; }
}
