using AspireApp.ApiService.Notifications.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace AspireApp.ApiService.Notifications.Infrastructure.Services;

/// <summary>
/// Firebase Cloud Messaging service implementation
/// </summary>
public class FirebaseFCMService : DomainService, IFirebaseFCMService
{
    private readonly ILogger<FirebaseFCMService> _logger;

    public FirebaseFCMService(ILogger<FirebaseFCMService> logger)
    {
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
            var message = new Message
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
            _logger.LogInformation("Successfully sent FCM message: {Response}", response);
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
            var message = new MulticastMessage
            {
                Tokens = tokenList,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, cancellationToken);
            
            for (int i = 0; i < tokenList.Count; i++)
            {
                results[tokenList[i]] = response.Responses[i].IsSuccess;
            }

            _logger.LogInformation("Sent FCM multicast message. Success: {SuccessCount}, Failure: {FailureCount}",
                response.SuccessCount, response.FailureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending FCM multicast message");
            foreach (var token in tokenList)
            {
                results[token] = false;
            }
        }

        return results;
    }
}

