using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Notifications.Domain.Interfaces;

/// <summary>
/// Interface for Firebase Authentication service.
/// Handles Firebase user management and token operations.
/// </summary>
public interface IFirebaseAuthService : IDomainService
{
    /// <summary>
    /// Creates a Firebase user
    /// </summary>
    Task<string> CreateUserAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Firebase UID for a user email
    /// </summary>
    Task<string?> GetFirebaseUidAsync(string email, CancellationToken cancellationToken = default);
}

