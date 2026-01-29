using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CryptoJackpot.Audit.Domain.Models;

/// <summary>
/// Contains response information for audit logging.
/// </summary>
public class AuditResponseInfo
{
    [BsonElement("statusCode")]
    public int? StatusCode { get; set; }

    [BsonElement("duration")]
    public long? DurationMs { get; set; }

    [BsonElement("body")]
    public BsonDocument? Body { get; set; }
}
