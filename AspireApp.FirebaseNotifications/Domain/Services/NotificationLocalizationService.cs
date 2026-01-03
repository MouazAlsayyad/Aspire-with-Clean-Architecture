using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Interfaces;

namespace AspireApp.FirebaseNotifications.Domain.Services;

/// <summary>
/// Domain service for notification localization.
/// Provides user-based localization by retrieving user language preferences.
/// </summary>
public class NotificationLocalizationService : DomainService, INotificationLocalizationService
{
    private readonly IUserRepository _userRepository;

    public NotificationLocalizationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<LocalizedNotificationContent> GetLocalizedContentAsync(
        Guid userId,
        string titleKey,
        string bodyKey,
        params object[] args)
    {
        var language = await GetUserLanguageAsync(userId);
        return NotificationLocalization.GetContent(language, titleKey, bodyKey, null, args);
    }

    public async Task<string> GetUserLanguageAsync(Guid userId)
    {
        var user = await _userRepository.GetAsync(userId, cancellationToken: default);
        return user?.Language ?? "en";
    }

    public void ReloadResources()
    {
        NotificationLocalization.ReloadResources();
    }
}

