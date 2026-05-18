using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NativeAdapter;

internal static class ApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5000"),
        Timeout = TimeSpan.FromSeconds(30)
    };

    public static float[] CallApi<TRequest>(string endpoint, TRequest request, JsonTypeInfo<TRequest> requestTypeInfo)
        where TRequest : class
    {
        var json = JsonSerializer.Serialize(request, requestTypeInfo);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = _httpClient.PostAsync(endpoint, content).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var responseJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var result = JsonSerializer.Deserialize(responseJson, ApiJsonContext.Default.CalcResponse);
        return result?.Result ?? [];
    }
}

public class BiRequest
{
    public int NCount { get; set; }
    public float[] High { get; set; } = [];
    public float[] Low { get; set; } = [];
}

public class DuanRequest
{
    public int NCount { get; set; }
    public float[] Bi { get; set; } = [];
    public float[] High { get; set; } = [];
    public float[] Low { get; set; } = [];
}

public class ZsRequest
{
    public int NCount { get; set; }
    public float[] Bi { get; set; } = [];
    public float[] High { get; set; } = [];
    public float[] Low { get; set; } = [];
}

public class CalcResponse
{
    [JsonPropertyName("result")]
    public float[] Result { get; set; } = [];
}

[JsonSerializable(typeof(BiRequest))]
[JsonSerializable(typeof(DuanRequest))]
[JsonSerializable(typeof(ZsRequest))]
[JsonSerializable(typeof(CalcResponse))]
internal partial class ApiJsonContext : JsonSerializerContext { }