using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Domain.Services;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;

namespace AspireApp.FirebaseNotifications.Infrastructure.Services;

/// <summary>
/// Firebase Authentication service implementation
/// </summary>
public class FirebaseAuthService : DomainService, IFirebaseAuthService
{
    private readonly ILogger<FirebaseAuthService> _logger;

    public FirebaseAuthService(ILogger<FirebaseAuthService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateUserAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRecordArgs = new UserRecordArgs
            {
                Email = email,
                EmailVerified = false,
                Disabled = false
            };

            var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs, cancellationToken);
            _logger.LogInformation("Successfully created Firebase user {Uid} for email {Email}", userRecord.Uid, email);
            return userRecord.Uid;
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
        {
            _logger.LogWarning("Firebase user already exists for email {Email}", email);
            var existingUser = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email, cancellationToken);
            return existingUser.Uid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Firebase user for email {Email}", email);
            throw;
        }
    }

    public async Task<string?> GetFirebaseUidAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email, cancellationToken);
            return userRecord.Uid;
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            _logger.LogWarning("Firebase user not found for email {Email}", email);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Firebase UID for email {Email}", email);
            return null;
        }
    }
}

