namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Strategy for generating membership subscription email templates
/// </summary>
public interface IMembershipEmailTemplateStrategy : IEmailTemplateStrategy
{
    /// <summary>
    /// Gets the membership subscription email template
    /// </summary>
    string GetTemplate(
        string playerName,
        string membershipDate,
        string paymentLink);
}

