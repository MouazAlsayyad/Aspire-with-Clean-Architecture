namespace AspireApp.Email.Infrastructure.Templates;

/// <summary>
/// Stripe onboarding email template
/// </summary>
public static class OnboardingTemplate
{
    public static string GetTemplate(string onboardingUrl)
    {
        return $@"
<div style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: white;"">
    <div style=""max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden;"">
        
        <!-- Header: Black background -->
        <div style=""background-color: black; color: white; text-align: center; padding: 20px;"">
            <h1 style=""margin: 0; font-size: 24px; font-weight: bold;"">Stripe Onboarding</h1>
        </div>
        
        <!-- Content: Light gray background -->
        <div style=""padding: 20px; background-color: #f9f9f9; margin: 20px; border: 1px solid #dddddd; border-radius: 8px;"">
            <p style=""margin: 0 0 15px 0; font-size: 20px; color: #333333;"">
                Complete Your Stripe Onboarding
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                To start receiving payouts, you need to complete your Stripe account setup.
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                Click the button below to continue the onboarding process:
            </p>
            <div style=""text-align: center; margin: 20px 0;"">
                <a href=""{onboardingUrl}"" style=""display: inline-block; padding: 12px 30px; background-color: #635bff; color: white; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;"">
                    Complete Stripe Onboarding
                </a>
            </div>
            <p style=""margin: 0 0 10px 0; font-size: 14px; color: #666666;"">
                This is a secure process handled by Stripe. You'll need to provide:
            </p>
            <ul style=""margin: 0 0 15px 20px; font-size: 14px; color: #666666;"">
                <li>Business information</li>
                <li>Bank account details</li>
                <li>Identity verification</li>
            </ul>
            <p style=""margin: 0; font-size: 14px; color: #666666;"">
                If you have any questions, please contact our support team.
            </p>
        </div>
        
        <!-- Footer: Black background with copyright -->
        <div style=""text-align: center; padding: 20px; background-color: black; color: white; font-size: 14px;"">
            <p style=""margin: 0;"">&copy; 2025. All rights reserved.</p>
        </div>
    </div>
</div>";
    }
}

