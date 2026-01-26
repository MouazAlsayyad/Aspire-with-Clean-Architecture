using Refit;

namespace AspireApp.Email.Infrastructure.RefitClients;

public interface ISendGridApi
{
    [Post("/v3/mail/send")]
    Task<HttpResponseMessage> SendEmailAsync([Body] SendGridMessageRequest request, [Header("Authorization")] string authorization, CancellationToken cancellationToken = default);
}

public class SendGridMessageRequest
{
    public List<SendGridPersonalization> Personalizations { get; set; } = new();
    public SendGridEmailAddress From { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public List<SendGridContent> Content { get; set; } = new();
    public List<SendGridAttachment>? Attachments { get; set; }
}

public class SendGridPersonalization
{
    public List<SendGridEmailAddress> To { get; set; } = new();
    public List<SendGridEmailAddress>? Bcc { get; set; }
}

public class SendGridEmailAddress
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class SendGridContent
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class SendGridAttachment
{
    public string Content { get; set; } = string.Empty; // Base64
    public string Filename { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Disposition { get; set; } = "attachment";
}
