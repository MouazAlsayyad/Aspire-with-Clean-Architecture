namespace AspireApp.Payment.Domain.Models;

/// <summary>
/// Request model for refunding a payment
/// </summary>
public class RefundPaymentRequest
{
    public Guid PaymentId { get; set; }
    public string ExternalReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

