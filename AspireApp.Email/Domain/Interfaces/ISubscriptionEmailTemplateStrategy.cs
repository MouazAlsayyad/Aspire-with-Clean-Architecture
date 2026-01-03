namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Strategy for generating subscription invoice email templates
/// </summary>
public interface ISubscriptionEmailTemplateStrategy : IEmailTemplateStrategy
{
    /// <summary>
    /// Gets the subscription invoice email template
    /// </summary>
    string GetTemplate(
        string subscriptionType,
        string length);
}

