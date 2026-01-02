namespace AspireApp.Email.Infrastructure.Templates;

/// <summary>
/// OTP verification email template
/// </summary>
public static class OTPTemplate
{
    public static string GetTemplate(string clubName, string otp)
    {
        return $@"
<div style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: white;"">
    <div style=""max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden;"">
        
        <!-- Header: Black background -->
        <div style=""background-color: black; color: white; text-align: center; padding: 20px;"">
            <h1 style=""margin: 0; font-size: 24px; font-weight: bold;"">{clubName}</h1>
        </div>
        
        <!-- Content: Light gray background -->
        <div style=""padding: 20px; background-color: #f9f9f9; margin: 20px; border: 1px solid #dddddd; border-radius: 8px;"">
            <p style=""margin: 0 0 15px 0; font-size: 20px; color: #333333;"">
                OTP Verification
            </p>
            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #333333;"">
                Your one-time password (OTP) for <strong>{clubName}</strong> is:
            </p>
            <div style=""text-align: center; margin: 20px 0;"">
                <div style=""display: inline-block; padding: 20px 40px; background-color: #f0f0f0; border: 2px dashed #333; border-radius: 5px;"">
                    <span style=""font-size: 32px; font-weight: bold; color: #333; letter-spacing: 5px;"">{otp}</span>
                </div>
            </div>
            <p style=""margin: 0 0 10px 0; font-size: 14px; color: #d9534f;"">
                ⚠️ This OTP is valid for 5 minutes only.
            </p>
            <p style=""margin: 0; font-size: 14px; color: #666666;"">
                If you didn't request this OTP, please ignore this email.
            </p>
        </div>
        
        <!-- Footer: Black background with copyright -->
        <div style=""text-align: center; padding: 20px; background-color: black; color: white; font-size: 14px;"">
            <p style=""margin: 0;"">&copy; 2025 {clubName}. All rights reserved.</p>
        </div>
    </div>
</div>";
    }
}

