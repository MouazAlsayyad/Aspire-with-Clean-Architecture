using AspireApp.Domain.Shared.Entities;
using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Entities;

/// <summary>
/// Payment transaction record for audit trail
/// </summary>
public class PaymentTransaction : BaseEntity
{
    private PaymentTransaction() { } // For EF Core

    public PaymentTransaction(
        Guid paymentId,
        TransactionType type,
        decimal amount,
        PaymentStatus status,
        string? response = null)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Type = type;
        Amount = amount;
        Status = status;
        Response = response;
        TransactionDate = DateTime.UtcNow;
    }

    public new Guid Id { get; private set; }
    
    /// <summary>
    /// Reference to parent payment
    /// </summary>
    public Guid PaymentId { get; private set; }
    
    /// <summary>
    /// Navigation property to parent payment
    /// </summary>
    public Payment Payment { get; private set; } = null!;
    
    /// <summary>
    /// Type of transaction
    /// </summary>
    public TransactionType Type { get; private set; }
    
    /// <summary>
    /// Transaction amount
    /// </summary>
    public decimal Amount { get; private set; }
    
    /// <summary>
    /// Transaction status
    /// </summary>
    public PaymentStatus Status { get; private set; }
    
    /// <summary>
    /// Raw API response from payment provider
    /// </summary>
    public string? Response { get; private set; }
    
    /// <summary>
    /// Date and time of transaction
    /// </summary>
    public DateTime TransactionDate { get; private set; }
}

