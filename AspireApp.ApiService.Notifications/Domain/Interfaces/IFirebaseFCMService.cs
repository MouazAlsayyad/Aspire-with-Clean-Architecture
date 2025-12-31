using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Notifications.Domain.Interfaces;

/// <summary>
/// Interface for Firebase Cloud Messaging service.
/// Handles FCM message sending operations.
/// </summary>
public interface IFirebaseFCMService : IDomainService
{
    /// <summary>
    /// Sends a notification to a single FCM token
    /// </summary>
    Task<bool> SendToTokenAsync(
        string token,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to multiple FCM tokens
    /// </summary>
    Task<Dictionary<string, bool>> SendToTokensAsync(
        IEnumerable<string> tokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}

