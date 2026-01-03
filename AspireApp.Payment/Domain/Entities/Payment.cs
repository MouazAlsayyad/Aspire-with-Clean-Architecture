using AspireApp.Domain.Shared.Entities;
using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Events;

namespace AspireApp.Payment.Domain.Entities;

/// <summary>
/// Payment entity representing a payment transaction
/// </summary>
public class Payment : BaseEntity
{
    private Payment() 
    {
        Transactions = new List<PaymentTransaction>();
    } // For EF Core

    public Payment(
        string orderNumber,
        PaymentMethod method,
        decimal amount,
        string currency,
        Guid? userId = null,
        string? customerEmail = null,
        string? customerPhone = null,
        string? metadata = null)
    {
        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        Method = method;
        Status = PaymentStatus.Pending;
        Amount = amount;
        Currency = currency;
        UserId = userId;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        Metadata = metadata;
        Transactions = new List<PaymentTransaction>();
        
        AddDomainEvent(new PaymentCreatedEvent(Id, OrderNumber, Method, Amount, Currency));
    }

    public new Guid Id { get; private set; }
    
    /// <summary>
    /// Unique order reference number
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;
    
    /// <summary>
    /// Payment method used
    /// </summary>
    public PaymentMethod Method { get; private set; }
    
    /// <summary>
    /// Current payment status
    /// </summary>
    public PaymentStatus Status { get; private set; }
    
    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; private set; }
    
    /// <summary>
    /// Currency code (USD, AED, SAR, etc.)
    /// </summary>
    public string Currency { get; private set; } = string.Empty;
    
    /// <summary>
    /// External reference from payment provider (Stripe PaymentIntentId, Tabby PaymentId)
    /// </summary>
    public string? ExternalReference { get; private set; }
    
    /// <summary>
    /// User ID who made the payment
    /// </summary>
    public Guid? UserId { get; private set; }
    
    /// <summary>
    /// Customer email address
    /// </summary>
    public string? CustomerEmail { get; private set; }
    
    /// <summary>
    /// Customer phone number
    /// </summary>
    public string? CustomerPhone { get; private set; }
    
    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; private set; }
    
    /// <summary>
    /// Payment transaction history
    /// </summary>
    public ICollection<PaymentTransaction> Transactions { get; private set; }

    /// <summary>
    /// Updates the payment status
    /// </summary>
    public void UpdateStatus(PaymentStatus newStatus, string? externalReference = null)
    {
        var oldStatus = Status;
        Status = newStatus;
        
        if (!string.IsNullOrEmpty(externalReference))
        {
            ExternalReference = externalReference;
        }

        // Raise appropriate domain events
        switch (newStatus)
        {
            case PaymentStatus.Processing:
                AddDomainEvent(new PaymentProcessingEvent(Id, OrderNumber, Method));
                break;
            case PaymentStatus.Authorized:
                AddDomainEvent(new PaymentAuthorizedEvent(Id, OrderNumber, Amount, ExternalReference));
                break;
            case PaymentStatus.Succeeded:
                AddDomainEvent(new PaymentSucceededEvent(
                    Id, OrderNumber, Amount, Currency, Method, CustomerEmail, CustomerPhone));
                break;
            case PaymentStatus.Failed:
                AddDomainEvent(new PaymentFailedEvent(Id, OrderNumber, Method, "Payment failed"));
                break;
            case PaymentStatus.Refunded:
            case PaymentStatus.PartiallyRefunded:
                AddDomainEvent(new PaymentRefundedEvent(Id, OrderNumber, Amount, newStatus == PaymentStatus.PartiallyRefunded));
                break;
        }
    }

    /// <summary>
    /// Adds a transaction record
    /// </summary>
    public void AddTransaction(
        TransactionType type,
        decimal amount,
        PaymentStatus status,
        string? response = null)
    {
        var transaction = new PaymentTransaction(Id, type, amount, status, response);
        Transactions.Add(transaction);
    }
}

