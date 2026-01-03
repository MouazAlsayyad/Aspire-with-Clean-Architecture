namespace AspireApp.Payment.Domain.Enums;

/// <summary>
/// Payment methods supported by the system
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Cash payment (manual confirmation required)
    /// </summary>
    Cash = 0,
    
    /// <summary>
    /// Stripe payment (immediate card payment)
    /// </summary>
    Stripe = 1,
    
    /// <summary>
    /// Tabby payment (Buy Now, Pay Later)
    /// </summary>
    Tabby = 2
}

