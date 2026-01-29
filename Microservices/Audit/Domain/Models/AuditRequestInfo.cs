using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CryptoJackpot.Audit.Domain.Models;

/// <summary>
/// Contains HTTP request information for audit logging.
/// </summary>
public class AuditRequestInfo
{
    [BsonElement("endpoint")]
    public string? Endpoint { get; set; }

    [BsonElement("method")]
    public string? Method { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("headers")]
    public BsonDocument? Headers { get; set; }

    [BsonElement("body")]
    public BsonDocument? Body { get; set; }
}
