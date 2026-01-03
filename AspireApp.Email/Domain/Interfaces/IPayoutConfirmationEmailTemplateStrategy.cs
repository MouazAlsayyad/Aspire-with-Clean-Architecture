namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Strategy for generating payout confirmation email templates
/// </summary>
public interface IPayoutConfirmationEmailTemplateStrategy : IEmailTemplateStrategy
{
    /// <summary>
    /// Gets the payout confirmation email template
    /// </summary>
    string GetTemplate(double amount);
}

