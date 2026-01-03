using AspireApp.Domain.Shared.Common;

namespace AspireApp.Payment.Domain.Events;

/// <summary>
/// Domain event raised when a payment is refunded
/// </summary>
public class PaymentRefundedEvent : IDomainEvent
{
    public PaymentRefundedEvent(
        Guid paymentId,
        string orderNumber,
        decimal amount,
        bool isPartial)
    {
        PaymentId = paymentId;
        OrderNumber = orderNumber;
        Amount = amount;
        IsPartial = isPartial;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid PaymentId { get; }
    public string OrderNumber { get; }
    public decimal Amount { get; }
    public bool IsPartial { get; }
    public DateTime OccurredOn { get; }
}
