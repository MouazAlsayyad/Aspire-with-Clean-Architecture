using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Application.DTOs;

/// <summary>
/// DTO representing a payment
/// </summary>
public class PaymentDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public Guid? UserId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

