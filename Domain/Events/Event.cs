namespace CryptoJackpot.Domain.Core.Events;

public abstract class Event
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public DateTime Timestamp { get; protected set; }
    
    /// <summary>
    /// Unique correlation ID for distributed tracing across microservices.
    /// </summary>
    public string CorrelationId { get; set; }

    protected Event()
    { 
        Timestamp = DateTime.UtcNow;
        CorrelationId = Guid.NewGuid().ToString();
    }
}
