using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace CryptoJackpot.Identity.Infra.IoC.Serializers;

/// <summary>
/// Deserializer for raw JSON messages from external systems that don't use MassTransit envelope.
/// Used for consuming events from Keycloak SPI which publishes plain JSON.
/// </summary>
/// <typeparam name="T">The type to deserialize to.</typeparam>
public class RawJsonDeserializer<T> : IDeserializer<T> where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull || data.IsEmpty)
            return null!;

        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json, Options)!;
    }
}

