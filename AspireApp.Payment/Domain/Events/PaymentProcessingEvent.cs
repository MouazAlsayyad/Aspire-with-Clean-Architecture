using AspireApp.Domain.Shared.Common;
using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Events;

/// <summary>
/// Domain event raised when a payment starts processing
/// </summary>
public class PaymentProcessingEvent : IDomainEvent
{
    public PaymentProcessingEvent(
        Guid paymentId,
        string orderNumber,
        PaymentMethod method)
    {
        PaymentId = paymentId;
        OrderNumber = orderNumber;
        Method = method;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid PaymentId { get; }
    public string OrderNumber { get; }
    public PaymentMethod Method { get; }
    public DateTime OccurredOn { get; }
}
