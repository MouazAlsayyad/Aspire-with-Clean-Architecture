namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Strategy for generating booking confirmation email templates
/// </summary>
public interface IBookingEmailTemplateStrategy : IEmailTemplateStrategy
{
    /// <summary>
    /// Gets the booking confirmation email template
    /// </summary>
    string GetTemplate(
        string playerName,
        string courtName,
        string bookingDate,
        string fromTime,
        string paymentLink);
}

