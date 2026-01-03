namespace AspireApp.Payment.Domain.Models;

/// <summary>
/// Request model for processing a payment
/// </summary>
public class ProcessPaymentRequest
{
    public Guid PaymentId { get; set; }
    public string? ExternalReference { get; set; }
    public Dictionary<string, string>? AdditionalData { get; set; }
}

