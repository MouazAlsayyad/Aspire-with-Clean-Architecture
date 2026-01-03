using AspireApp.Domain.Shared.Common;

namespace AspireApp.Payment.Domain.Events;

/// <summary>
/// Domain event raised when a payment is authorized (Tabby specific)
/// </summary>
public class PaymentAuthorizedEvent : IDomainEvent
{
    public PaymentAuthorizedEvent(
        Guid paymentId,
        string orderNumber,
        decimal amount,
        string? externalReference)
    {
        PaymentId = paymentId;
        OrderNumber = orderNumber;
        Amount = amount;
        ExternalReference = externalReference;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid PaymentId { get; }
    public string OrderNumber { get; }
    public decimal Amount { get; }
    public string? ExternalReference { get; }
    public DateTime OccurredOn { get; }
}
