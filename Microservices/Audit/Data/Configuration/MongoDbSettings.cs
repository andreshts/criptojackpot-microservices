namespace CryptoJackpot.Audit.Data.Configuration;

/// <summary>
/// MongoDB configuration settings.
/// </summary>
public class MongoDbSettings
{
    public const string SectionName = "MongoDB";

    /// <summary>
    /// MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database name.
    /// </summary>
    public string DatabaseName { get; set; } = "cryptojackpot_audit";

    /// <summary>
    /// Audit logs collection name.
    /// </summary>
    public string AuditLogsCollection { get; set; } = "audit_logs";

    /// <summary>
    /// TTL in days for audit logs (0 = no expiration).
    /// </summary>
    public int RetentionDays { get; set; } = 365;
}
