using AspireApp.Domain.Shared.Interfaces;

namespace AspireApp.ApiService.Notifications.Domain.Interfaces;

/// <summary>
/// Interface for notification localization service.
/// Provides user-based localization for notification content.
/// </summary>
public interface INotificationLocalizationService : IDomainService
{
    /// <summary>
    /// Gets localized notification content based on user's language preference
    /// </summary>
    Task<LocalizedNotificationContent> GetLocalizedContentAsync(
        Guid userId,
        string titleKey,
        string bodyKey,
        params object[] args);

    /// <summary>
    /// Gets user's language preference
    /// </summary>
    Task<string> GetUserLanguageAsync(Guid userId);

    /// <summary>
    /// Reloads localization resources (useful for development)
    /// </summary>
    void ReloadResources();
}

/// <summary>
/// Localized notification content DTO
/// </summary>
public class LocalizedNotificationContent
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public string? ActionUrl { get; set; }
}

