using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Models;
using AspireApp.Payment.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace AspireApp.Payment.Infrastructure.Strategies;

/// <summary>
/// Strategy for Tabby payments (Buy Now, Pay Later)
/// </summary>
public class TabbyPaymentStrategy : ITabbyPaymentStrategy
{
    private readonly TabbyPaymentService _tabbyService;
    private readonly ILogger<TabbyPaymentStrategy> _logger;

    public PaymentMethod Method => PaymentMethod.Tabby;

    public TabbyPaymentStrategy(
        TabbyPaymentService tabbyService,
        ILogger<TabbyPaymentStrategy> logger)
    {
        _tabbyService = tabbyService;
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Tabby payment for order {OrderNumber}", request.OrderNumber);

            var session = await _tabbyService.CreateSessionAsync(
                request.Amount,
                request.Currency,
                request.CustomerPhone ?? "+971500000000",
                request.CustomerEmail ?? "customer@example.com",
                request.CustomerName ?? "Customer",
                request.OrderNumber,
                request.ProductName ?? "Product",
                request.ProductImage,
                request.SuccessUrl ?? "https://example.com/success",
                request.CancelUrl ?? "https://example.com/cancel",
                cancellationToken);

            // Get the payment URL from available products
            var paymentUrl = session.Configuration?.AvailableProducts?.FirstOrDefault()?.WebUrl;

            return PaymentResult.Successful(
                PaymentStatus.Processing,
                externalReference: session.Payment.Id,
                paymentUrl: paymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Tabby payment for order {OrderNumber}", request.OrderNumber);
            return PaymentResult.Failed(ex.Message);
        }
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing Tabby payment {PaymentId}", request.PaymentId);

            if (string.IsNullOrEmpty(request.ExternalReference))
            {
                return PaymentResult.Failed("External reference (Tabby PaymentId) is required");
            }

            var payment = await _tabbyService.GetPaymentAsync(
                request.ExternalReference,
                cancellationToken);

            var status = payment.Status.ToLower() switch
            {
                "authorized" => PaymentStatus.Authorized,
                "closed" => PaymentStatus.Succeeded,
                "captured" => PaymentStatus.Succeeded,
                "rejected" => PaymentStatus.Failed,
                "expired" => PaymentStatus.Cancelled,
                "new" => PaymentStatus.Processing,
                _ => PaymentStatus.Pending
            };

            return PaymentResult.Successful(status, request.ExternalReference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Tabby payment {PaymentId}", request.PaymentId);
            return PaymentResult.Failed(ex.Message);
        }
    }

    public async Task<RefundResult> RefundPaymentAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing Tabby refund for payment {PaymentId}", request.PaymentId);

            var refund = await _tabbyService.RefundPaymentAsync(
                request.ExternalReference,
                request.Amount,
                request.Reason,
                cancellationToken);

            return RefundResult.Successful(refund.Id, request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding Tabby payment {PaymentId}", request.PaymentId);
            return RefundResult.Failed(ex.Message);
        }
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(
        string externalReference,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _tabbyService.GetPaymentAsync(
                externalReference,
                cancellationToken);

            var status = payment.Status.ToLower() switch
            {
                "authorized" => PaymentStatus.Authorized,
                "closed" => PaymentStatus.Succeeded,
                "captured" => PaymentStatus.Succeeded,
                "rejected" => PaymentStatus.Failed,
                "expired" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Pending
            };

            return PaymentStatusResult.Successful(
                status,
                decimal.Parse(payment.Amount),
                payment.Currency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Tabby payment status {ExternalReference}", externalReference);
            return PaymentStatusResult.Failed(ex.Message);
        }
    }
}

