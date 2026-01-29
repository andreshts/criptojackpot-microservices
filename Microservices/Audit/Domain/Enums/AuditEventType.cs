namespace CryptoJackpot.Audit.Domain.Enums;

/// <summary>
/// Defines the types of events that can be audited in the system.
/// </summary>
public enum AuditEventType
{
    // Identity Events
    UserLogin = 100,
    UserLogout = 101,
    UserRegistration = 102,
    PasswordChange = 103,
    PasswordReset = 104,
    TokenRefresh = 105,
    LoginFailed = 106,
    AccountLocked = 107,
    AccountUnlocked = 108,
    
    // Wallet Events
    WalletCreated = 200,
    WalletUpdated = 201,
    DepositInitiated = 210,
    DepositCompleted = 211,
    DepositFailed = 212,
    WithdrawalInitiated = 220,
    WithdrawalCompleted = 221,
    WithdrawalFailed = 222,
    
    // CoinPayments Events
    CoinPaymentTransactionCreated = 300,
    CoinPaymentIpnReceived = 301,
    CoinPaymentTransactionCompleted = 302,
    CoinPaymentTransactionFailed = 303,
    CoinPaymentTransactionPending = 304,
    
    // Lottery Events
    LotteryTicketPurchased = 400,
    LotteryDrawStarted = 401,
    LotteryDrawCompleted = 402,
    LotteryWinnerSelected = 403,
    
    // Order Events
    OrderCreated = 500,
    OrderUpdated = 501,
    OrderCompleted = 502,
    OrderCancelled = 503,
    
    // Notification Events
    NotificationSent = 600,
    NotificationFailed = 601,
    
    // System Events
    SystemError = 900,
    SystemStartup = 901,
    SystemShutdown = 902,
    ConfigurationChanged = 903
}
