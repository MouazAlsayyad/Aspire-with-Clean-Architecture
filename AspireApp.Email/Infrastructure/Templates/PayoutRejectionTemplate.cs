namespace AspireApp.Email.Infrastructure.Templates;

/// <summary>
/// Payout rejection email template
/// </summary>
public static class PayoutRejectionTemplate
{
    public static string GetTemplate(string tenantName)
    {
        return $@"
<div style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: white;"">
    <div style=""max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden;"">
        
        <!-- Header: Black background -->
        <div style=""background-color: black; color: white; text-align: center; padding: 20px;"">
            <h1 style=""margin: 0; font-size: 24px; font-weight: bold;"">{tenantName}</h1>
        </div>
        
        <!-- Content: Light gray background -->
        <div style=""padding: 20px; background-color: #f9f9f9; margin: 20px; border: 1px solid #dddddd; border-radius: 8px;"">
            <p style=""margin: 0 0 15px 0; font-size: 20px; color: #333333;"">
                Payout Request Update
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                Dear {tenantName},
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                We regret to inform you that your recent payout request has not been approved at this time.
            </p>
            <div style=""text-align: center; margin: 20px 0; padding: 15px; background-color: #f8d7da; border: 2px solid #dc3545; border-radius: 5px;"">
                <span style=""font-size: 16px; color: #721c24;"">❌ Payout Request Rejected</span>
            </div>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                Please contact our support team for more information or to resolve any issues.
            </p>
            <p style=""margin: 0; font-size: 14px; color: #666666;"">
                Email: support@sender.com
            </p>
        </div>
        
        <!-- Footer: Black background with copyright -->
        <div style=""text-align: center; padding: 20px; background-color: black; color: white; font-size: 14px;"">
            <p style=""margin: 0;"">&copy; 2025 {tenantName}. All rights reserved.</p>
        </div>
    </div>
</div>";
    }
}

