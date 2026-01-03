using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Application.DTOs;

/// <summary>
/// DTO representing the result of a payment operation
/// </summary>
public class PaymentResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ExternalReference { get; set; }
    public string? PaymentUrl { get; set; }
    public PaymentDto? Payment { get; set; }
}

