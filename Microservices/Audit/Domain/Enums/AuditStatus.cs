namespace CryptoJackpot.Audit.Domain.Enums;

/// <summary>
/// Represents the outcome status of an audited operation.
/// </summary>
public enum AuditStatus
{
    Success = 1,
    Failed = 2,
    Pending = 3,
    Cancelled = 4,
    Unknown = 99
}
