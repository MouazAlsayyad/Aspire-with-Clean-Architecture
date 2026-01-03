using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Models;
using Microsoft.Extensions.Logging;

namespace AspireApp.Payment.Infrastructure.Strategies;

/// <summary>
/// Strategy for cash payments (manual confirmation required)
/// </summary>
public class CashPaymentStrategy : ICashPaymentStrategy
{
    private readonly ILogger<CashPaymentStrategy> _logger;

    public PaymentMethod Method => PaymentMethod.Cash;

    public CashPaymentStrategy(ILogger<CashPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public Task<PaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating cash payment for order {OrderNumber}", request.OrderNumber);

        // Cash payments are marked as pending until manually confirmed
        var result = PaymentResult.Successful(
            PaymentStatus.Pending,
            externalReference: null,
            paymentUrl: null);

        return Task.FromResult(result);
    }

    public Task<PaymentResult> ProcessPaymentAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing cash payment {PaymentId}", request.PaymentId);

        // Cash payments remain pending until manual confirmation
        // This method would typically be called by an admin to confirm receipt
        var result = PaymentResult.Successful(PaymentStatus.Succeeded);

        return Task.FromResult(result);
    }

    public Task<RefundResult> RefundPaymentAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing cash refund for payment {PaymentId}", request.PaymentId);

        // Cash refunds are handled manually
        var result = RefundResult.Successful(
            refundId: $"CASH-REFUND-{Guid.NewGuid():N}",
            amount: request.Amount);

        return Task.FromResult(result);
    }

    public Task<PaymentStatusResult> GetPaymentStatusAsync(
        string externalReference,
        CancellationToken cancellationToken = default)
    {
        // Cash payments don't have external references
        var result = PaymentStatusResult.Failed("Cash payments do not have external status tracking");
        return Task.FromResult(result);
    }
}

