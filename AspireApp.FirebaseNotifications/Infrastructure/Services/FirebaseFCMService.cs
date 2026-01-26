using AspireApp.FirebaseNotifications.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.FirebaseNotifications.Infrastructure.RefitClients;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace AspireApp.FirebaseNotifications.Infrastructure.Services;

/// <summary>
/// Firebase Cloud Messaging service implementation using Refit
/// </summary>
public class FirebaseFCMService : DomainService, IFirebaseFCMService
{
    private readonly IFirebaseFcmApi _fcmApi;
    private readonly ILogger<FirebaseFCMService> _logger;

    public FirebaseFCMService(
        IFirebaseFcmApi fcmApi,
        ILogger<FirebaseFCMService> logger)
    {
        _fcmApi = fcmApi;
        _logger = logger;
    }

    public async Task<bool> SendToTokenAsync(
        string token,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var projectId = FirebaseApp.DefaultInstance.Options.ProjectId;
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            var request = new FirebaseMessageRequest
            {
                Message = new FirebaseMessage
                {
                    Token = token,
                    Notification = new FirebaseNotification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data
                }
            };

            var response = await _fcmApi.SendMessageAsync(projectId, request, $"Bearer {accessToken}", cancellationToken);
            _logger.LogInformation("Successfully sent FCM message: {Name}", response.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending FCM message to token {Token}", token);
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
        var results = new Dictionary<string, bool>();
        var tokenList = tokens.ToList();

        if (!tokenList.Any())
            return results;

        try
        {
            var projectId = FirebaseApp.DefaultInstance.Options.ProjectId;
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            // Send requests in parallel
            var tasks = tokenList.Select(async token =>
            {
                try
                {
                    var request = new FirebaseMessageRequest
                    {
                        Message = new FirebaseMessage
                        {
                            Token = token,
                            Notification = new FirebaseNotification
                            {
                                Title = title,
                                Body = body
                            },
                            Data = data
                        }
                    };

                    await _fcmApi.SendMessageAsync(projectId, request, $"Bearer {accessToken}", cancellationToken);
                    return (Token: token, Success: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending FCM message to token {Token}", token);
                    return (Token: token, Success: false);
                }
            });

            var sendResults = await Task.WhenAll(tasks);

            foreach (var result in sendResults)
            {
                results[result.Token] = result.Success;
            }

            var successCount = sendResults.Count(r => r.Success);
            var failureCount = sendResults.Count(r => !r.Success);

            _logger.LogInformation("Sent FCM multicast message. Success: {SuccessCount}, Failure: {FailureCount}",
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing FCM multicast message");
            foreach (var token in tokenList)
            {
                results[token] = false;
            }
        }

        return results;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var credential = FirebaseApp.DefaultInstance.Options.Credential;

        // This relies on Google.Apis.Auth which FirebaseAdmin builds upon.
        // We need to request the correct scope for FCM v1.
        if (credential is GoogleCredential googleCredential)
        {
            if (googleCredential.IsCreateScopedRequired)
            {
                googleCredential = googleCredential.CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            }
            return await googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
        }

        throw new InvalidOperationException("Could not retrieve GoogleCredential from FirebaseApp");
    }
}

