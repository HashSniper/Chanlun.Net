using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using TdxQuantNet.Models;

namespace TdxQuantNet;

internal static class Helper
{
    internal static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    internal static readonly JsonSerializerOptions InputJsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
    
    internal static string ConvertTimeFormat(string time)
    {
        var format = time.Length switch
        {
            8 => "yyyyMMdd",
            14 => "yyyyMMddHHmmSS",
            _ => throw new ArgumentException("时间格式不正确，应为 YYYYMMDD 或 YYYYMMDDHHMMSS")
        };
        var dt = DateTime.ParseExact(time, format, CultureInfo.InvariantCulture);
        return dt.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 获取当前时间 (string "yyyyMMdd")
    /// </summary>
    /// <returns></returns>
    internal static string GetNowTimeFormat()
    {
        var now = DateTime.UtcNow + TimeSpan.FromHours(8);
        return now.ToString("yyyyMMdd");
    }

    internal static JsonNode? ParsePtrJson(IntPtr ptr, out string error)
    {
        error = "";
        var response = Marshal.PtrToStringUTF8(ptr);
        if (response is null) return null;
        var callModel = JsonSerializer.Deserialize<CallErrorModel>(response, JsonSerializerOptions);
        if (callModel is null) return null;
        // ReSharper disable once InvertIf
        if (callModel.ErrorId != 0)
        {
            error = callModel.Error ?? "";
            return null;
        }

        return JsonNode.Parse(response);
    }
    
    internal static T? ParsePtrJson<T>(IntPtr ptr, out string error) where T : notnull
    {
        error = "";
        var response = Marshal.PtrToStringUTF8(ptr);
        if (response is null) return default;
        var callModel = JsonSerializer.Deserialize<CallErrorModel>(response, JsonSerializerOptions);
        if (callModel is null) return default;

        // ReSharper disable once InvertIf
        if (callModel.ErrorId != 0)
        {
            error = callModel.Error ?? "";
            return default;
        }

        return JsonSerializer.Deserialize<T>(response, JsonSerializerOptions);
    }
}
