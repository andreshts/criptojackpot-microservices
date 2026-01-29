using System.Text.Json;
using MongoDB.Bson;

namespace CryptoJackpot.Audit.Domain.Helpers;

/// <summary>
/// BSON document helper utilities for MongoDB operations.
/// </summary>
public static class BsonHelper
{
    /// <summary>
    /// Converts an object to a BsonDocument.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>A BsonDocument representation of the object, or null if conversion fails.</returns>
    public static BsonDocument? ToBsonDocument(object? value)
    {
        if (value == null)
            return null;

        try
        {
            var json = JsonSerializer.Serialize(value);
            return BsonDocument.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a JSON string to a BsonDocument. Returns null if parsing fails.
    /// </summary>
    public static BsonDocument? ParseFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return BsonDocument.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a BsonDocument to a JSON string. Returns null if conversion fails.
    /// </summary>
    public static string? ToJson(BsonDocument? document)
    {
        if (document == null)
            return null;

        try
        {
            return document.ToJson();
        }
        catch
        {
            return null;
        }
    }
}
