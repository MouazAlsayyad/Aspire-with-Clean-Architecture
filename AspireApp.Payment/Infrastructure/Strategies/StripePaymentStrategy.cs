using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Models;
using AspireApp.Payment.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace AspireApp.Payment.Infrastructure.Strategies;

/// <summary>
/// Strategy for Stripe payments
/// </summary>
public class StripePaymentStrategy : IStripePaymentStrategy
{
    private readonly StripePaymentService _stripeService;
    private readonly ILogger<StripePaymentStrategy> _logger;

    public PaymentMethod Method => PaymentMethod.Stripe;

    public StripePaymentStrategy(
        StripePaymentService stripeService,
        ILogger<StripePaymentStrategy> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Stripe payment for order {OrderNumber}", request.OrderNumber);

            var session = await _stripeService.CreateCheckoutSessionAsync(
                request.Amount,
                request.Currency,
                request.CustomerEmail ?? "customer@example.com",
                request.CustomerName ?? "Customer",
                request.OrderNumber,
                request.ProductName ?? "Product",
                request.ProductImage,
                request.SuccessUrl ?? "https://example.com/success",
                request.CancelUrl ?? "https://example.com/cancel",
                cancellationToken);

            return PaymentResult.Successful(
                PaymentStatus.Processing,
                externalReference: session.PaymentIntentId,
                paymentUrl: session.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Stripe payment for order {OrderNumber}", request.OrderNumber);
            return PaymentResult.Failed(ex.Message);
        }
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing Stripe payment {PaymentId}", request.PaymentId);

            if (string.IsNullOrEmpty(request.ExternalReference))
            {
                return PaymentResult.Failed("External reference (PaymentIntentId) is required");
            }

            var paymentIntent = await _stripeService.GetPaymentIntentAsync(
                request.ExternalReference,
                cancellationToken);

            var status = paymentIntent.Status switch
            {
                "succeeded" => PaymentStatus.Succeeded,
                "processing" => PaymentStatus.Processing,
                "requires_payment_method" => PaymentStatus.Failed,
                "requires_confirmation" => PaymentStatus.Processing,
                "requires_action" => PaymentStatus.Processing,
                "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Failed
            };

            return PaymentResult.Successful(status, request.ExternalReference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe payment {PaymentId}", request.PaymentId);
            return PaymentResult.Failed(ex.Message);
        }
    }

    public async Task<RefundResult> RefundPaymentAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing Stripe refund for payment {PaymentId}", request.PaymentId);

            var refund = await _stripeService.CreateRefundAsync(
                request.ExternalReference,
                request.Amount,
                request.Reason,
                cancellationToken);

            return RefundResult.Successful(refund.Id, request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding Stripe payment {PaymentId}", request.PaymentId);
            return RefundResult.Failed(ex.Message);
        }
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(
        string externalReference,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentIntent = await _stripeService.GetPaymentIntentAsync(
                externalReference,
                cancellationToken);

            var status = paymentIntent.Status switch
            {
                "succeeded" => PaymentStatus.Succeeded,
                "processing" => PaymentStatus.Processing,
                "requires_payment_method" => PaymentStatus.Failed,
                "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Pending
            };

            return PaymentStatusResult.Successful(
                status,
                (decimal)paymentIntent.Amount / 100, // Convert from cents
                paymentIntent.Currency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Stripe payment status {ExternalReference}", externalReference);
            return PaymentStatusResult.Failed(ex.Message);
        }
    }
}

