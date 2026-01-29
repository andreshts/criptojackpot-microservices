using System.Text.Json;

namespace CryptoJackpot.Domain.Core.Helpers;

/// <summary>
/// JSON serialization helper utilities.
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes an object to JSON string. Returns null if serialization fails.
    /// </summary>
    public static string? SerializeToJson(object? value, JsonSerializerOptions? options = null)
    {
        if (value == null)
            return null;

        try
        {
            return JsonSerializer.Serialize(value, options ?? DefaultOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Deserializes a JSON string to an object. Returns default if deserialization fails.
    /// </summary>
    public static T? DeserializeFromJson<T>(string? json, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }
        catch
        {
            return default;
        }
    }
}
