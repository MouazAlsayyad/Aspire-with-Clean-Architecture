namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Strategy for generating payout rejection email templates
/// </summary>
public interface IPayoutRejectionEmailTemplateStrategy : IEmailTemplateStrategy
{
    // Uses base GetTemplate() method with no additional parameters
}

