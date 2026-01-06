using AspireApp.ApiService.Domain.Services;
using AspireApp.Domain.Shared.Common;
using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;

namespace AspireApp.Payment.Domain.Services;

/// <summary>
/// Domain manager for payment business logic
/// </summary>
public class PaymentManager : DomainService, IPaymentManager
{
    private const decimal MinimumPaymentAmount = 0.01m;
    private const decimal MaximumPaymentAmount = 999999.99m;

    public void ValidatePaymentRequest(decimal amount, string currency, string orderNumber)
    {
        // Validate amount
        if (amount < MinimumPaymentAmount)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput($"Payment amount must be at least {MinimumPaymentAmount}"));
        }

        if (amount > MaximumPaymentAmount)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput($"Payment amount cannot exceed {MaximumPaymentAmount}"));
        }

        // Validate currency
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Currency is required"));
        }

        if (currency.Length != 3)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Currency must be a 3-letter ISO code (e.g., USD, AED, SAR)"));
        }

        // Validate order number
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Order number is required"));
        }
    }

    public string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        return $"PAY-{timestamp}-{random}";
    }

    public void ValidateRefundRequest(Entities.Payment payment, decimal refundAmount)
    {
        if (!CanBeRefunded(payment.Status))
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput($"Payment with status {payment.Status} cannot be refunded"));
        }

        if (refundAmount <= 0)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Refund amount must be greater than zero"));
        }

        if (refundAmount > payment.Amount.Amount)
        {
            throw new DomainException(
                DomainErrors.General.InvalidInput("Refund amount cannot exceed the original payment amount"));
        }
    }

    public bool CanBeRefunded(PaymentStatus status)
    {
        return status == PaymentStatus.Succeeded || status == PaymentStatus.PartiallyRefunded;
    }
}

