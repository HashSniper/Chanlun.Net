using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record IpoInfoItem
{
    [JsonPropertyName("MaxSG")]
    public required decimal MaxSg { get; init; }
    
    [JsonPropertyName("PE_Issue")]
    public required decimal PeIssue { get; init; }
    
    [JsonPropertyName("SGCode")]
    public required string SgCode { get; init; }
    
    [JsonPropertyName("SGDate")]
    public required string SgDate { get; init; }
    
    [JsonPropertyName("SGPrice")]
    public required decimal SgPrice { get; init; }
    
    [JsonPropertyName("code")]
    public required string Code { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("setcode")]
    public required string SetCode { get; init; }
}
