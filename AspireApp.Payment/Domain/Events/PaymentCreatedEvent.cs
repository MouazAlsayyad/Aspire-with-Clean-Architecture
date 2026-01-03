using AspireApp.Domain.Shared.Common;
using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Events;

/// <summary>
/// Domain event raised when a payment is created
/// </summary>
public class PaymentCreatedEvent : IDomainEvent
{
    public PaymentCreatedEvent(
        Guid paymentId,
        string orderNumber,
        PaymentMethod method,
        decimal amount,
        string currency)
    {
        PaymentId = paymentId;
        OrderNumber = orderNumber;
        Method = method;
        Amount = amount;
        Currency = currency;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid PaymentId { get; }
    public string OrderNumber { get; }
    public PaymentMethod Method { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateTime OccurredOn { get; }
}

