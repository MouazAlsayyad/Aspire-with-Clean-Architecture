using AspireApp.ApiService.Infrastructure.DomainEvents;
using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Models;
using AspireApp.Notifications.Domain.Services;
using AspireApp.Payment.Domain.Events;
using AspireApp.Payment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.Payment.Infrastructure.Handlers;

/// <summary>
/// Handles payment domain events and sends notifications
/// </summary>
public class PaymentEventHandler :
    IDomainEventHandler<PaymentSucceededEvent>,
    IDomainEventHandler<PaymentFailedEvent>,
    IDomainEventHandler<PaymentRefundedEvent>,
    IDomainEventHandler<PaymentAuthorizedEvent>
{
    private readonly NotificationOrchestrator _notificationOrchestrator;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentEventHandler> _logger;

    public PaymentEventHandler(
        NotificationOrchestrator notificationOrchestrator,
        IPaymentRepository paymentRepository,
        ILogger<PaymentEventHandler> logger)
    {
        _notificationOrchestrator = notificationOrchestrator;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles payment succeeded event - sends success notification via all channels
    /// </summary>
    public async Task HandleAsync(PaymentSucceededEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Skip if no contact information is available
            if (string.IsNullOrEmpty(domainEvent.CustomerEmail) && string.IsNullOrEmpty(domainEvent.CustomerPhone))
            {
                _logger.LogWarning(
                    "Payment {OrderNumber} succeeded but no customer contact information available for notification",
                    domainEvent.OrderNumber);
                return;
            }

            var notificationRequest = new NotificationRequest
            {
                Recipient = domainEvent.CustomerEmail ?? domainEvent.CustomerPhone ?? string.Empty,
                Subject = "Payment Successful",
                Body = $"Your payment of {domainEvent.Amount} {domainEvent.Currency} for order {domainEvent.OrderNumber} was successful. Payment method: {domainEvent.Method}. Thank you for your payment!",
                Channels = new[] { NotificationChannel.All },
                Metadata = new Dictionary<string, string>
                {
                    ["PaymentId"] = domainEvent.PaymentId.ToString(),
                    ["OrderNumber"] = domainEvent.OrderNumber,
                    ["Amount"] = domainEvent.Amount.ToString("F2"),
                    ["Currency"] = domainEvent.Currency,
                    ["PaymentMethod"] = domainEvent.Method.ToString(),
                    ["EventType"] = "PaymentSucceeded"
                }
            };

            var results = await _notificationOrchestrator.SendAsync(notificationRequest, cancellationToken);

            _logger.LogInformation(
                "Sent payment success notification for order {OrderNumber} via {ChannelCount} channels",
                domainEvent.OrderNumber, results.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending payment success notification for order {OrderNumber}",
                domainEvent.OrderNumber);
        }
    }

    /// <summary>
    /// Handles payment failed event - sends failure notification
    /// </summary>
    public async Task HandleAsync(PaymentFailedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Fetch payment details to get customer contact information
            var payment = await _paymentRepository.GetAsync(domainEvent.PaymentId, false, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning(
                    "Payment {PaymentId} not found when handling failed event",
                    domainEvent.PaymentId);
                return;
            }

            // Skip if no contact information is available
            if (string.IsNullOrEmpty(payment.CustomerEmail) && string.IsNullOrEmpty(payment.CustomerPhone))
            {
                _logger.LogWarning(
                    "Payment {OrderNumber} failed but no customer contact information available for notification",
                    domainEvent.OrderNumber);
                return;
            }

            var notificationRequest = new NotificationRequest
            {
                Recipient = payment.CustomerEmail ?? payment.CustomerPhone ?? string.Empty,
                Subject = "Payment Failed",
                Body = $"Your payment for order {domainEvent.OrderNumber} failed. Reason: {domainEvent.Reason}. Please try again or contact support if the issue persists.",
                Channels = new[] { NotificationChannel.All },
                Metadata = new Dictionary<string, string>
                {
                    ["PaymentId"] = domainEvent.PaymentId.ToString(),
                    ["OrderNumber"] = domainEvent.OrderNumber,
                    ["PaymentMethod"] = domainEvent.Method.ToString(),
                    ["Reason"] = domainEvent.Reason,
                    ["EventType"] = "PaymentFailed"
                }
            };

            var results = await _notificationOrchestrator.SendAsync(notificationRequest, cancellationToken);

            _logger.LogInformation(
                "Sent payment failure notification for order {OrderNumber} via {ChannelCount} channels",
                domainEvent.OrderNumber, results.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending payment failure notification for order {OrderNumber}",
                domainEvent.OrderNumber);
        }
    }

    /// <summary>
    /// Handles payment refunded event - sends refund confirmation
    /// </summary>
    public async Task HandleAsync(PaymentRefundedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Fetch payment details to get customer contact information
            var payment = await _paymentRepository.GetAsync(domainEvent.PaymentId, false, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning(
                    "Payment {PaymentId} not found when handling refunded event",
                    domainEvent.PaymentId);
                return;
            }

            // Skip if no contact information is available
            if (string.IsNullOrEmpty(payment.CustomerEmail) && string.IsNullOrEmpty(payment.CustomerPhone))
            {
                _logger.LogWarning(
                    "Payment {OrderNumber} refunded but no customer contact information available for notification",
                    domainEvent.OrderNumber);
                return;
            }

            var refundType = domainEvent.IsPartial ? "partial refund" : "full refund";
            var notificationRequest = new NotificationRequest
            {
                Recipient = payment.CustomerEmail ?? payment.CustomerPhone ?? string.Empty,
                Subject = domainEvent.IsPartial ? "Partial Refund Processed" : "Refund Processed",
                Body = $"A {refundType} of {domainEvent.Amount} {payment.Currency} has been processed for order {domainEvent.OrderNumber}. The refund will appear in your account within 5-10 business days.",
                Channels = new[] { NotificationChannel.All },
                Metadata = new Dictionary<string, string>
                {
                    ["PaymentId"] = domainEvent.PaymentId.ToString(),
                    ["OrderNumber"] = domainEvent.OrderNumber,
                    ["RefundAmount"] = domainEvent.Amount.ToString("F2"),
                    ["Currency"] = payment.Currency,
                    ["IsPartial"] = domainEvent.IsPartial.ToString(),
                    ["EventType"] = "PaymentRefunded"
                }
            };

            var results = await _notificationOrchestrator.SendAsync(notificationRequest, cancellationToken);

            _logger.LogInformation(
                "Sent payment refund notification for order {OrderNumber} via {ChannelCount} channels",
                domainEvent.OrderNumber, results.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending payment refund notification for order {OrderNumber}",
                domainEvent.OrderNumber);
        }
    }

    /// <summary>
    /// Handles payment authorized event - sends authorization notification (Tabby specific)
    /// </summary>
    public async Task HandleAsync(PaymentAuthorizedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Fetch payment details to get customer contact information
            var payment = await _paymentRepository.GetAsync(domainEvent.PaymentId, false, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning(
                    "Payment {PaymentId} not found when handling authorized event",
                    domainEvent.PaymentId);
                return;
            }

            // Skip if no contact information is available
            if (string.IsNullOrEmpty(payment.CustomerEmail) && string.IsNullOrEmpty(payment.CustomerPhone))
            {
                _logger.LogWarning(
                    "Payment {OrderNumber} authorized but no customer contact information available for notification",
                    domainEvent.OrderNumber);
                return;
            }

            var notificationRequest = new NotificationRequest
            {
                Recipient = payment.CustomerEmail ?? payment.CustomerPhone ?? string.Empty,
                Subject = "Payment Authorized",
                Body = $"Your payment of {domainEvent.Amount} {payment.Currency} for order {domainEvent.OrderNumber} has been authorized and is being processed. You will receive another notification once the payment is completed.",
                Channels = new[] { NotificationChannel.All },
                Metadata = new Dictionary<string, string>
                {
                    ["PaymentId"] = domainEvent.PaymentId.ToString(),
                    ["OrderNumber"] = domainEvent.OrderNumber,
                    ["Amount"] = domainEvent.Amount.ToString("F2"),
                    ["Currency"] = payment.Currency,
                    ["ExternalReference"] = domainEvent.ExternalReference ?? string.Empty,
                    ["EventType"] = "PaymentAuthorized"
                }
            };

            var results = await _notificationOrchestrator.SendAsync(notificationRequest, cancellationToken);

            _logger.LogInformation(
                "Sent payment authorization notification for order {OrderNumber} via {ChannelCount} channels",
                domainEvent.OrderNumber, results.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending payment authorization notification for order {OrderNumber}",
                domainEvent.OrderNumber);
        }
    }
}

