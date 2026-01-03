using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Models;

namespace AspireApp.Payment.Domain.Interfaces;

/// <summary>
/// Base interface for payment strategies
/// </summary>
public interface IPaymentStrategy
{
    /// <summary>
    /// Payment method handled by this strategy
    /// </summary>
    PaymentMethod Method { get; }
    
    /// <summary>
    /// Creates a payment session/intent
    /// </summary>
    Task<PaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes/captures the payment
    /// </summary>
    Task<PaymentResult> ProcessPaymentAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refunds a payment
    /// </summary>
    Task<RefundResult> RefundPaymentAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current payment status from external provider
    /// </summary>
    Task<PaymentStatusResult> GetPaymentStatusAsync(
        string externalReference,
        CancellationToken cancellationToken = default);
}

