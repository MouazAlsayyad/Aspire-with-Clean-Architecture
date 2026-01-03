using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Domain.Options;
using Microsoft.Extensions.Options;

namespace AspireApp.Email.Infrastructure.Templates.Strategies;

/// <summary>
/// Strategy implementation for payout confirmation email templates
/// </summary>
public class PayoutConfirmationEmailTemplateStrategy : IPayoutConfirmationEmailTemplateStrategy
{
    private readonly string _applicationTitle;

    public PayoutConfirmationEmailTemplateStrategy(IOptions<EmailOptions> emailOptions)
    {
        _applicationTitle = emailOptions.Value.ApplicationTitle;
    }

    public string GetTemplate()
    {
        throw new NotSupportedException("Use GetTemplate with parameters for payout confirmation emails.");
    }

    public string GetTemplate(double amount)
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
                Payout Confirmed
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                Your payout request has been approved and processed successfully!
            </p>
            <div style=""text-align: center; margin: 20px 0;"">
                <div style=""display: inline-block; padding: 20px 40px; background-color: #d4edda; border: 2px solid #28a745; border-radius: 5px;"">
                    <span style=""font-size: 24px; font-weight: bold; color: #155724;"">Amount: ${amount:F2}</span>
                </div>
            </div>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                The payout details and invoice are attached to this email.
            </p>
            <p style=""margin: 0; font-size: 14px; color: #666666;"">
                Please allow 3-5 business days for the funds to appear in your account.
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

