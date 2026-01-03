using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Models;

/// <summary>
/// Result of a payment status query
/// </summary>
public class PaymentStatusResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }

    public static PaymentStatusResult Successful(
        PaymentStatus status,
        decimal amount,
        string currency)
    {
        return new PaymentStatusResult
        {
            Success = true,
            Status = status,
            Amount = amount,
            Currency = currency
        };
    }

    public static PaymentStatusResult Failed(string errorMessage)
    {
        return new PaymentStatusResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

