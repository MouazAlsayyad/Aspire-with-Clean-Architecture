using AspireApp.Payment.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace AspireApp.Payment.Infrastructure.Services;

/// <summary>
/// Service for interacting with Stripe payment API
/// </summary>
public class StripePaymentService
{
    private readonly StripeOptions _options;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IOptions<StripeOptions> options,
        ILogger<StripePaymentService> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        // Configure Stripe API key
        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    /// <summary>
    /// Creates a Stripe Checkout session
    /// </summary>
    public async Task<Session> CreateCheckoutSessionAsync(
        decimal amount,
        string currency,
        string customerEmail,
        string customerName,
        string orderNumber,
        string productName,
        string? productImage,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency.ToLower(),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = productName,
                                Images = string.IsNullOrEmpty(productImage) 
                                    ? null 
                                    : new List<string> { productImage }
                            },
                            UnitAmount = (long)(amount * 100) // Convert to cents
                        },
                        Quantity = 1
                    }
                ],
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = customerEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "order_number", orderNumber },
                    { "customer_name", customerName }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Created Stripe checkout session {SessionId} for order {OrderNumber}",
                session.Id, orderNumber);

            return session;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating Stripe checkout session for order {OrderNumber}", orderNumber);
            throw;
        }
    }

    /// <summary>
    /// Gets the status of a Stripe Checkout session
    /// </summary>
    public async Task<Session> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);
            return session;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error retrieving Stripe session {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Refunds a Stripe payment
    /// </summary>
    public async Task<Refund> CreateRefundAsync(
        string paymentIntentId,
        decimal? amount = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = amount.HasValue ? (long)(amount.Value * 100) : null, // null = full refund
                Reason = reason
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Created Stripe refund {RefundId} for payment intent {PaymentIntentId}",
                refund.Id, paymentIntentId);

            return refund;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating refund for payment intent {PaymentIntentId}", paymentIntentId);
            throw;
        }
    }

    /// <summary>
    /// Gets a payment intent
    /// </summary>
    public async Task<PaymentIntent> GetPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
            return paymentIntent;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error retrieving payment intent {PaymentIntentId}", paymentIntentId);
            throw;
        }
    }
}

