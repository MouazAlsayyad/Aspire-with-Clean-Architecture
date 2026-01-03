using AspireApp.Domain.Shared.Common;
using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Events;

/// <summary>
/// Domain event raised when a payment succeeds
/// </summary>
public class PaymentSucceededEvent : IDomainEvent
{
    public PaymentSucceededEvent(
        Guid paymentId,
        string orderNumber,
        decimal amount,
        string currency,
        PaymentMethod method,
        string? customerEmail,
        string? customerPhone)
    {
        PaymentId = paymentId;
        OrderNumber = orderNumber;
        Amount = amount;
        Currency = currency;
        Method = method;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        OccurredOn = DateTime.UtcNow;
    }

    public Guid PaymentId { get; }
    public string OrderNumber { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public PaymentMethod Method { get; }
    public string? CustomerEmail { get; }
    public string? CustomerPhone { get; }
    public DateTime OccurredOn { get; }
}
