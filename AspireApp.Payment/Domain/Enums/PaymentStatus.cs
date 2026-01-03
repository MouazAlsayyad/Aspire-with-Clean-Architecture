namespace AspireApp.Payment.Domain.Enums;

/// <summary>
/// Payment status states
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment has been created but not yet processed
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Payment is being processed
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Payment has been authorized (Tabby specific - not yet captured)
    /// </summary>
    Authorized = 2,
    
    /// <summary>
    /// Payment completed successfully
    /// </summary>
    Succeeded = 3,
    
    /// <summary>
    /// Payment failed
    /// </summary>
    Failed = 4,
    
    /// <summary>
    /// Payment was cancelled
    /// </summary>
    Cancelled = 5,
    
    /// <summary>
    /// Payment was fully refunded
    /// </summary>
    Refunded = 6,
    
    /// <summary>
    /// Payment was partially refunded
    /// </summary>
    PartiallyRefunded = 7
}

