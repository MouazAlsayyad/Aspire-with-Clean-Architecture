using AspireApp.Notifications.Domain.Enums;
using AspireApp.Notifications.Domain.Interfaces;
using AspireApp.Notifications.Domain.Models;
using Microsoft.Extensions.Logging;

namespace AspireApp.Notifications.Domain.Services;

/// <summary>
/// Orchestrates notification sending across multiple channels
/// </summary>
public class NotificationOrchestrator
{
    private readonly INotificationStrategyFactory _strategyFactory;
    private readonly ILogger<NotificationOrchestrator> _logger;

    public NotificationOrchestrator(
        INotificationStrategyFactory strategyFactory,
        ILogger<NotificationOrchestrator> logger)
    {
        _strategyFactory = strategyFactory;
        _logger = logger;
    }

    /// <summary>
    /// Sends notifications via the specified channels
    /// </summary>
    public async Task<IEnumerable<NotificationResult>> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Channels == null || request.Channels.Length == 0)
        {
            _logger.LogWarning("No notification channels specified in request");
            return Array.Empty<NotificationResult>();
        }

        var results = new List<NotificationResult>();

        foreach (var channel in request.Channels)
        {
            try
            {
                var strategy = _strategyFactory.GetStrategy(channel);
                var result = await strategy.SendAsync(request, cancellationToken);
                results.Add(result);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Successfully sent notification via {Channel} to {Recipient}",
                        channel, request.Recipient);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send notification via {Channel} to {Recipient}: {Error}",
                        channel, request.Recipient, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending notification via {Channel} to {Recipient}",
                    channel, request.Recipient);
                
                results.Add(NotificationResult.Failed(channel, ex.Message));
            }
        }

        return results;
    }
}

