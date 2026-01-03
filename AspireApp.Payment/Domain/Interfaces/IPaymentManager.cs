using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Interfaces;

/// <summary>
/// Domain manager for payment business logic
/// </summary>
public interface IPaymentManager : IDomainService
{
    /// <summary>
    /// Validates payment request data
    /// </summary>
    void ValidatePaymentRequest(decimal amount, string currency, string orderNumber);
    
    /// <summary>
    /// Generates a unique order number
    /// </summary>
    string GenerateOrderNumber();
    
    /// <summary>
    /// Validates refund request
    /// </summary>
    void ValidateRefundRequest(Entities.Payment payment, decimal refundAmount);
    
    /// <summary>
    /// Determines if a payment can be refunded
    /// </summary>
    bool CanBeRefunded(PaymentStatus status);
}

