namespace AspireApp.Payment.Domain.Enums;

/// <summary>
/// Types of payment transactions
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Initial payment authorization
    /// </summary>
    Authorization = 0,
    
    /// <summary>
    /// Capture of authorized payment
    /// </summary>
    Capture = 1,
    
    /// <summary>
    /// Direct charge (no authorization)
    /// </summary>
    Charge = 2,
    
    /// <summary>
    /// Full or partial refund
    /// </summary>
    Refund = 3,
    
    /// <summary>
    /// Payment cancellation
    /// </summary>
    Cancellation = 4
}

