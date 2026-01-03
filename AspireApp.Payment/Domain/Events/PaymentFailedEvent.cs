using AspireApp.Domain.Shared.Common;
using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Events;

/// <summary>
/// Domain event raised when a payment fails
/// </summary>
public class PaymentFailedEvent : IDomainEvent
{
    public PaymentFailedEvent(
        Guid paymentId,
        string orderNumber,
        PaymentMethod method,
        string reason)
    {
        PaymentId = paymentId;
        OrderNumber = orderNumber;
        Method = method;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid PaymentId { get; }
    public string OrderNumber { get; }
    public PaymentMethod Method { get; }
    public string Reason { get; }
    public DateTime OccurredOn { get; }
}
