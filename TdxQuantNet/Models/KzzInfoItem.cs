using System.Text.Json.Serialization;

namespace TdxQuantNet.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record KzzInfoItem
{
    [JsonPropertyName("SetCode")]
    public required string SetCode { get; init; }
    
    [JsonPropertyName("KZZCode")]
    public required string KzzCode { get; init; }
    
    [JsonPropertyName("HSCode")]
    public required string HsCode { get; init; }
    
    [JsonPropertyName("ZGPrice")]
    public required decimal ZgPrice { get; init; }
    
    [JsonPropertyName("CurRate")]
    public required decimal CurRate { get; init; }
    
    [JsonPropertyName("RestScope")]
    public required decimal RestScope { get; init; }
    
    [JsonPropertyName("PutBack")]
    public required decimal PutBack { get; init; }
    
    [JsonPropertyName("ForceRedeem")]
    public required decimal ForceRedeem { get; init; }
    
    [JsonPropertyName("ZGDate")]
    public required string ZgDate { get; init; }
    
    [JsonPropertyName("EndPrice")]
    public required decimal EndPrice { get; init; }
    
    [JsonPropertyName("EndDate")]
    public required string EndDate { get; init; }
    
    [JsonPropertyName("ZGRate")]
    public required decimal ZgRate { get; init; }
    
    [JsonPropertyName("RealValue")]
    public required decimal RealValue { get; init; }
    
    [JsonPropertyName("ExpireYield")]
    public required decimal ExpireYield { get; init; }
    
    [JsonPropertyName("KZZScore")]
    public required string KzzScore { get; init; }
    
    [JsonPropertyName("HSScore")]
    public required string HsScore { get; init; }
    
    [JsonPropertyName("RedeemDate")]
    public required string RedeemDate { get; init; }
    
    [JsonPropertyName("RedeemPrice")]
    public required decimal RedeemPrice { get; init; }
    
    [JsonPropertyName("PutDate")]
    public required string PutDate { get; init; }
    
    [JsonPropertyName("PutPrice")]
    public required decimal PutPrice { get; init; }
    
    [JsonPropertyName("ZGCode")]
    public required string ZgCode { get; init; }
    
    [JsonPropertyName("AGNow")]
    public required decimal AgNow { get; init; }
    
    [JsonPropertyName("KZZNow")]
    public required decimal KzzNow { get; init; }
    
    [JsonPropertyName("KZZYj")]
    public required decimal KzzYj { get; init; }
    
    [JsonPropertyName("ZGValue")]
    public required decimal ZgValue { get; init; }
}
