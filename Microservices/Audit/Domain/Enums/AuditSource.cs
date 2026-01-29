namespace CryptoJackpot.Audit.Domain.Enums;

/// <summary>
/// Identifies the source microservice that generated the audit event.
/// </summary>
public enum AuditSource
{
    Identity = 1,
    Wallet = 2,
    Lottery = 3,
    Order = 4,
    Notification = 5,
    Winner = 6,
    System = 99
}
