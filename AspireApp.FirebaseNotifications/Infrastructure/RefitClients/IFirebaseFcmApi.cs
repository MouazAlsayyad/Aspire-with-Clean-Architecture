using Refit;
using System.Text.Json.Serialization;

namespace AspireApp.FirebaseNotifications.Infrastructure.RefitClients;

public interface IFirebaseFcmApi
{
    [Post("/v1/projects/{projectId}/messages:send")]
    Task<FirebaseMessageResponse> SendMessageAsync(string projectId, [Body] FirebaseMessageRequest request, [Header("Authorization")] string authorization, CancellationToken cancellationToken = default);
}

public class FirebaseMessageRequest
{
    [JsonPropertyName("message")]
    public FirebaseMessage Message { get; set; } = new();
}

public class FirebaseMessage
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("notification")]
    public FirebaseNotification? Notification { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, string>? Data { get; set; }
}

public class FirebaseNotification
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }
}

public class FirebaseMessageResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; } // The identifier of the message sent, e.g. projects/{project_id}/messages/{message_id}
}
