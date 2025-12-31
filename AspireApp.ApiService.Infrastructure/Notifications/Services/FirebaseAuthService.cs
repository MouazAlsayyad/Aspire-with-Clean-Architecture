using AspireApp.ApiService.Domain.Notifications.Interfaces;
using AspireApp.ApiService.Domain.Services;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Notifications.Services;

/// <summary>
/// Firebase Authentication service implementation.
/// Handles Firebase user management and token operations.
/// </summary>
public class FirebaseAuthService : DomainService, Domain.Notifications.Interfaces.IFirebaseAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FirebaseAuthService> _logger;
    private static bool _initialized = false;
    private static readonly object _initLock = new();

    public FirebaseAuthService(IConfiguration configuration, ILogger<FirebaseAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        if (_initialized)
            return;

        lock (_initLock)
        {
            if (_initialized)
                return;

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    var serviceAccountJson = GetServiceAccountJson();
                    if (string.IsNullOrEmpty(serviceAccountJson))
                    {
                        _logger.LogWarning("Firebase service account configuration not found.");
                        return;
                    }

                    var credential = GoogleCredential.FromJson(serviceAccountJson);
                    FirebaseApp.Create(new AppOptions() { Credential = credential });
                    _logger.LogInformation("Firebase initialized successfully");
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase");
            }
        }
    }

    private string GetServiceAccountJson()
    {
        var serviceAccount = _configuration.GetSection("Firebase:ServiceAccount");
        if (!serviceAccount.Exists())
            return string.Empty;

        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            type = serviceAccount["type"],
            project_id = serviceAccount["project_id"],
            private_key_id = serviceAccount["private_key_id"],
            private_key = serviceAccount["private_key"]?.Replace("\\n", "\n"),
            client_email = serviceAccount["client_email"],
            client_id = serviceAccount["client_id"],
            auth_uri = serviceAccount["auth_uri"],
            token_uri = serviceAccount["token_uri"],
            auth_provider_x509_cert_url = serviceAccount["auth_provider_x509_cert_url"],
            client_x509_cert_url = serviceAccount["client_x509_cert_url"],
            universe_domain = serviceAccount["universe_domain"]
        });

        return json;
    }

    public async Task<string> CreateUserAsync(string email, CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            throw new InvalidOperationException("Firebase not initialized");
        }

        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            var userRecord = await auth.CreateUserAsync(new UserRecordArgs
            {
                Email = email
            }, cancellationToken);

            return userRecord.Uid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Firebase user for email {Email}", email);
            throw;
        }
    }

    public async Task<string?> GetFirebaseUidAsync(string email, CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            return null;
        }

        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            var userRecord = await auth.GetUserByEmailAsync(email, cancellationToken);
            return userRecord.Uid;
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Firebase UID for email {Email}", email);
            return null;
        }
    }
}

