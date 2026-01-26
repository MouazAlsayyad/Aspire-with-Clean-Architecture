using Refit;

namespace AspireApp.Twilio.Infrastructure.RefitClients;

public interface ITwilioApi
{
    [Post("/2010-04-01/Accounts/{accountSid}/Messages.json")]
    Task<TwilioMessageResponse> SendMessageAsync(string accountSid, [Body(BodySerializationMethod.UrlEncoded)] TwilioMessageRequest request, [Header("Authorization")] string authorization, CancellationToken cancellationToken = default);
}

public class TwilioMessageRequest
{
    [AliasAs("To")]
    public string To { get; set; } = string.Empty;

    [AliasAs("From")]
    public string From { get; set; } = string.Empty;

    [AliasAs("Body")]
    public string? Body { get; set; }

    [AliasAs("ContentSid")]
    public string? ContentSid { get; set; }

    [AliasAs("ContentVariables")]
    public string? ContentVariables { get; set; }

    [AliasAs("StatusCallback")]
    public string? StatusCallback { get; set; }
}

public class TwilioMessageResponse
{
    [AliasAs("sid")]
    public string Sid { get; set; } = string.Empty;

    [AliasAs("status")]
    public string Status { get; set; } = string.Empty;

    [AliasAs("error_message")]
    public string? ErrorMessage { get; set; }

    [AliasAs("error_code")]
    public int? ErrorCode { get; set; }
}
