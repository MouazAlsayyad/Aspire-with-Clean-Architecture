namespace AspireApp.Email.Infrastructure.Templates;

/// <summary>
/// Password reset email template
/// </summary>
public static class PasswordResetTemplate
{
    public static string GetTemplate(string resetUrl)
    {
        return $@"
<div style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: white;"">
    <div style=""max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden;"">
        
        <!-- Header: Black background -->
        <div style=""background-color: black; color: white; text-align: center; padding: 20px;"">
            <h1 style=""margin: 0; font-size: 24px; font-weight: bold;"">Password Reset</h1>
        </div>
        
        <!-- Content: Light gray background -->
        <div style=""padding: 20px; background-color: #f9f9f9; margin: 20px; border: 1px solid #dddddd; border-radius: 8px;"">
            <p style=""margin: 0 0 15px 0; font-size: 20px; color: #333333;"">
                Password Reset Request
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                You have requested to reset your password for your account.
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                Click the button below to reset your password:
            </p>
            <div style=""text-align: center; margin: 20px 0;"">
                <a href=""{resetUrl}"" style=""display: inline-block; padding: 12px 30px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;"">
                    Reset Password
                </a>
            </div>
            <p style=""margin: 0 0 10px 0; font-size: 14px; color: #d9534f;"">
                ⚠️ This link will expire in 1 hour for security reasons.
            </p>
            <p style=""margin: 0; font-size: 14px; color: #666666;"">
                If you didn't request a password reset, please ignore this email and your password will remain unchanged.
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

