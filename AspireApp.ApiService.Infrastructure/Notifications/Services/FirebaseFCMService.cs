using AspireApp.ApiService.Domain.Notifications.Interfaces;
using AspireApp.ApiService.Domain.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Infrastructure.Notifications.Services;

/// <summary>
/// Firebase Cloud Messaging service implementation.
/// Handles sending push notifications via FCM.
/// </summary>
public class FirebaseFCMService : DomainService, Domain.Notifications.Interfaces.IFirebaseFCMService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FirebaseFCMService> _logger;
    private static bool _initialized = false;
    private static readonly object _initLock = new();

    public FirebaseFCMService(IConfiguration configuration, ILogger<FirebaseFCMService> logger)
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
                        _logger.LogWarning("Firebase service account configuration not found. Firebase notifications will be disabled.");
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

    public async Task<bool> SendToTokenAsync(
        string token,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("Firebase not initialized. Cannot send notification.");
            return false;
        }

        try
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>()
            };

            var messaging = FirebaseMessaging.DefaultInstance;
            var response = await messaging.SendAsync(message, cancellationToken);
            _logger.LogInformation("Successfully sent FCM message: {MessageId}", response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Failed to send FCM message to token {Token}. Error: {ErrorCode}", token, ex.ErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending FCM message to token {Token}", token);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> SendToTokensAsync(
        IEnumerable<string> tokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("Firebase not initialized. Cannot send notifications.");
            return tokens.ToDictionary(t => t, _ => false);
        }

        var results = new Dictionary<string, bool>();
        var tokenList = tokens.ToList();

        if (tokenList.Count == 0)
            return results;

        try
        {
            var messages = tokenList.Select(token => new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>()
            }).ToList();

            var messaging = FirebaseMessaging.DefaultInstance;
            var response = await messaging.SendAllAsync(messages, cancellationToken);

            // Map responses to results
            for (int i = 0; i < tokenList.Count && i < response.Responses.Count; i++)
            {
                var token = tokenList[i];
                var sendResponse = response.Responses[i];
                results[token] = sendResponse.IsSuccess;
                
                if (!sendResponse.IsSuccess)
                {
                    _logger.LogWarning("Failed to send to token {Token}: {Error}", token, sendResponse.Exception?.Message);
                }
            }

            _logger.LogInformation("Sent {SuccessCount} of {TotalCount} FCM messages", 
                results.Values.Count(r => r), tokenList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch FCM messages");
            // Mark all as failed
            foreach (var token in tokenList)
            {
                results[token] = false;
            }
        }

        return results;
    }
}

