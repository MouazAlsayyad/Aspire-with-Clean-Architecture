using AspireApp.ApiService.Domain.Notifications.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Notifications.Services;

/// <summary>
/// Background service that initializes notification localization at application startup
/// </summary>
public class NotificationLocalizationInitializer : BackgroundService
{
    private readonly ILogger<NotificationLocalizationInitializer> _logger;

    public NotificationLocalizationInitializer(ILogger<NotificationLocalizationInitializer> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Initializing notification localization system...");

            // Initialize the static localization system
            NotificationLocalization.Initialize();

            // Log available languages
            var languages = NotificationLocalization.GetAvailableLanguages();
            if (languages.Any())
            {
                _logger.LogInformation("Loaded localization for languages: {Languages}", string.Join(", ", languages));
            }
            else
            {
                _logger.LogWarning("No localization resources loaded. Notification localization may not work correctly.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing notification localization system");
        }

        // This service runs once at startup, so we return a completed task
        return Task.CompletedTask;
    }
}

