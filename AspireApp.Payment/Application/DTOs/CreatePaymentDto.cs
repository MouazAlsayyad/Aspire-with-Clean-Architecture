using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Application.DTOs;

/// <summary>
/// DTO for creating a payment
/// </summary>
public class CreatePaymentDto
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid? UserId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImage { get; set; }
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

