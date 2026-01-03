using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Models;

/// <summary>
/// Result of a payment operation
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ExternalReference { get; set; }
    public string? PaymentUrl { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }

    public static PaymentResult Successful(
        PaymentStatus status,
        string? externalReference = null,
        string? paymentUrl = null)
    {
        return new PaymentResult
        {
            Success = true,
            Status = status,
            ExternalReference = externalReference,
            PaymentUrl = paymentUrl
        };
    }

    public static PaymentResult Failed(string errorMessage, PaymentStatus status = PaymentStatus.Failed)
    {
        return new PaymentResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Status = status
        };
    }
}

