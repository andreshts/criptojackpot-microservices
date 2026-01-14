namespace CryptoJackpot.Domain.Core.Constants;

/// <summary>
/// Kafka topic names used across microservices.
/// Centralizes topic configuration to avoid hardcoded strings.
/// </summary>
public static class KafkaTopics
{
    // Identity Events
    public const string UserRegistered = "user-registered";
    public const string UserLoggedIn = "user-logged-in";
    public const string PasswordResetRequested = "password-reset-requested";
    public const string ReferralCreated = "referral-created";
    
    // Lottery Events
    public const string LotteryCreated = "lottery-created";
    public const string NumbersReserved = "numbers-reserved";
    public const string NumbersReleased = "numbers-released";
    public const string NumbersSold = "numbers-sold";
    
    // Order Events
    public const string OrderCreated = "order-created";
    public const string OrderCompleted = "order-completed";
    public const string OrderExpired = "order-expired";
    public const string OrderCancelled = "order-cancelled";
    public const string OrderTimeout = "order-timeout";
    
    // Notification Events
    public const string EmailSent = "email-sent";
    public const string NotificationFailed = "notification-failed";
    
    // Consumer Groups
    public const string NotificationGroup = "notification-group";
    public const string AnalyticsGroup = "analytics-group";
    public const string LotteryGroup = "lottery-group";
    public const string OrderGroup = "order-group";
}

