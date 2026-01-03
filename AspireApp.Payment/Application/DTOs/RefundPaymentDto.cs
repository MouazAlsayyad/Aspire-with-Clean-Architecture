namespace AspireApp.Payment.Application.DTOs;

/// <summary>
/// DTO for refunding a payment
/// </summary>
public class RefundPaymentDto
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

