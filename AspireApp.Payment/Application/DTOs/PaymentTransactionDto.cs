using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Application.DTOs;

/// <summary>
/// DTO representing a payment transaction
/// </summary>
public class PaymentTransactionDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime TransactionDate { get; set; }
}

