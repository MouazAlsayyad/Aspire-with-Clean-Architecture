namespace AspireApp.Payment.Application.DTOs;

/// <summary>
/// DTO for processing a payment
/// </summary>
public class ProcessPaymentDto
{
    public Guid PaymentId { get; set; }
    public string? ExternalReference { get; set; }
}

