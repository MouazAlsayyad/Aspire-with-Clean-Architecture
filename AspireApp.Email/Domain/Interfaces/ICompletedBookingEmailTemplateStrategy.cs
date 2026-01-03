namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Strategy for generating completed booking email templates
/// </summary>
public interface ICompletedBookingEmailTemplateStrategy : IEmailTemplateStrategy
{
    /// <summary>
    /// Gets the completed booking email template
    /// </summary>
    string GetTemplate(
        string bookingDate,
        string fromTime,
        double amount);
}

