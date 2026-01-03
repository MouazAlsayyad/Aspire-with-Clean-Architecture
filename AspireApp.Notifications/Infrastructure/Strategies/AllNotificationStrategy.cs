using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Interfaces;
using AspireApp.Notifications.Domain.Models;
using Microsoft.Extensions.Logging;

namespace AspireApp.Notifications.Infrastructure.Strategies;

/// <summary>
/// Strategy for sending notifications via all available channels
/// </summary>
public class AllNotificationStrategy : IAllNotificationStrategy
{
    private readonly ITwilioSmsNotificationStrategy _twilioSmsStrategy;
    private readonly ITwilioWhatsAppNotificationStrategy _twilioWhatsAppStrategy;
    private readonly IEmailNotificationStrategy _emailStrategy;
    private readonly IFirebaseNotificationStrategy _firebaseStrategy;
    private readonly ILogger<AllNotificationStrategy> _logger;

    public NotificationChannel Channel => NotificationChannel.All;

    public AllNotificationStrategy(
        ITwilioSmsNotificationStrategy twilioSmsStrategy,
        ITwilioWhatsAppNotificationStrategy twilioWhatsAppStrategy,
        IEmailNotificationStrategy emailStrategy,
        IFirebaseNotificationStrategy firebaseStrategy,
        ILogger<AllNotificationStrategy> logger)
    {
        _twilioSmsStrategy = twilioSmsStrategy;
        _twilioWhatsAppStrategy = twilioWhatsAppStrategy;
        _emailStrategy = emailStrategy;
        _firebaseStrategy = firebaseStrategy;
        _logger = logger;
    }

    public async Task<NotificationResult> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Execute all strategies in parallel
            var tasks = new[]
            {
                _twilioSmsStrategy.SendAsync(request, cancellationToken),
                _twilioWhatsAppStrategy.SendAsync(request, cancellationToken),
                _emailStrategy.SendAsync(request, cancellationToken),
                _firebaseStrategy.SendAsync(request, cancellationToken)
            };

            var results = await Task.WhenAll(tasks);

            // Check if at least one channel succeeded
            var anySuccess = results.Any(r => r.Success);
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            _logger.LogInformation(
                "All channels notification completed: {SuccessCount} succeeded, {FailureCount} failed",
                successCount, failureCount);

            if (anySuccess)
            {
                return NotificationResult.Successful(
                    NotificationChannel.All,
                    $"{successCount}/{results.Length} channels succeeded");
            }

            return NotificationResult.Failed(
                NotificationChannel.All,
                "All notification channels failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification via all channels");
            return NotificationResult.Failed(NotificationChannel.All, ex.Message);
        }
    }
}

