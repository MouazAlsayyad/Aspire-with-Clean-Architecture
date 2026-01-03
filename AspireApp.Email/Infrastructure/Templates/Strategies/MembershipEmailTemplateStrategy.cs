using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Domain.Options;
using Microsoft.Extensions.Options;

namespace AspireApp.Email.Infrastructure.Templates.Strategies;

/// <summary>
/// Strategy implementation for membership subscription email templates
/// </summary>
public class MembershipEmailTemplateStrategy : IMembershipEmailTemplateStrategy
{
    private readonly string _applicationTitle;

    public MembershipEmailTemplateStrategy(IOptions<EmailOptions> emailOptions)
    {
        _applicationTitle = emailOptions.Value.ApplicationTitle;
    }

    public string GetTemplate()
    {
        throw new NotSupportedException("Use GetTemplate with parameters for membership emails.");
    }

    public string GetTemplate(
        string playerName,
        string membershipDate,
        string paymentLink)
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
                Dear {playerName},
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                Your membership subscription has been initiated!
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                <strong>Membership Start Date:</strong> {membershipDate}
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                Please complete your payment using the link below:
            </p>
            <div style=""text-align: center; margin: 20px 0;"">
                <a href=""{paymentLink}"" style=""display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;"">
                    Complete Payment
                </a>
            </div>
            <p style=""margin: 0; font-size: 14px; color: #666666;"">
                Thank you for joining!
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

