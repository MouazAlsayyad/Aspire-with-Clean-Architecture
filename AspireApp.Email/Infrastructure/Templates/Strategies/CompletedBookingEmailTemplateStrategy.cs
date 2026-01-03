using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Domain.Options;
using Microsoft.Extensions.Options;

namespace AspireApp.Email.Infrastructure.Templates.Strategies;

/// <summary>
/// Strategy implementation for completed booking email templates
/// </summary>
public class CompletedBookingEmailTemplateStrategy : ICompletedBookingEmailTemplateStrategy
{
    private readonly string _applicationTitle;

    public CompletedBookingEmailTemplateStrategy(IOptions<EmailOptions> emailOptions)
    {
        _applicationTitle = emailOptions.Value.ApplicationTitle;
    }

    public string GetTemplate()
    {
        throw new NotSupportedException("Use GetTemplate with parameters for completed booking emails.");
    }

    public string GetTemplate(
        string bookingDate,
        string fromTime,
        double amount)
    {
        return $@"
<div style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: white;"">
    <div style=""max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden;"">
        
        <!-- Header: Black background -->
        <div style=""background-color: black; color: white; text-align: center; padding: 20px;"">
            <h1 style=""margin: 0; font-size: 24px; font-weight: bold;"">{_applicationTitle}</h1>
        </div>
        
        <!-- Content: Light gray background -->
        <div style=""padding: 20px; background-color: #f9f9f9; margin: 20px; border: 1px solid #dddddd; border-radius: 8px;"">
            <p style=""margin: 0 0 15px 0; font-size: 20px; color: #333333;"">
                Booking Completed
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                A booking has been completed successfully.
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                <strong>Booking Date:</strong> {bookingDate}<br>
                <strong>Time:</strong> {fromTime}<br>
                <strong>Amount Paid:</strong> ${amount:F2}
            </p>
            <p style=""margin: 0; font-size: 14px; color: #666666;"">
                Thank you!
            </p>
        </div>
        
        <!-- Footer: Black background with copyright -->
        <div style=""text-align: center; padding: 20px; background-color: black; color: white; font-size: 14px;"">
            <p style=""margin: 0;"">&copy; 2025 {_applicationTitle}. All rights reserved.</p>
        </div>
    </div>
</div>";
    }
}

