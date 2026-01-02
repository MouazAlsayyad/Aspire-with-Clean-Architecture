namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Interface for providing email HTML templates
/// </summary>
public interface IEmailTemplateProvider
{
    /// <summary>
    /// Gets the booking confirmation email template
    /// </summary>
    string GetBookingTemplate(
        string playerName,
        string courtName,
        string tenantName,
        string bookingDate,
        string fromTime,
        string paymentLink);

    /// <summary>
    /// Gets the completed booking email template (for tenants)
    /// </summary>
    string GetCompletedBookingTemplate(
        string tenantName,
        string bookingDate,
        string fromTime,
        double amount);

    /// <summary>
    /// Gets the membership subscription email template
    /// </summary>
    string GetMembershipTemplate(
        string playerName,
        string tenantName,
        string membershipDate,
        string paymentLink);

    /// <summary>
    /// Gets the OTP verification email template
    /// </summary>
    string GetOtpTemplate(
        string clubName,
        string otp);

    /// <summary>
    /// Gets the payout OTP verification email template
    /// </summary>
    string GetPayoutOtpTemplate(
        string clubName,
        string otp);

    /// <summary>
    /// Gets the payout confirmation email template
    /// </summary>
    string GetPayoutConfirmationTemplate(
        string tenantName,
        double amount);

    /// <summary>
    /// Gets the payout rejection email template
    /// </summary>
    string GetPayoutRejectionTemplate(
        string tenantName);

    /// <summary>
    /// Gets the subscription invoice email template
    /// </summary>
    string GetSubscriptionTemplate(
        string tenantName,
        string subscriptionType,
        string length);

    /// <summary>
    /// Gets the password reset email template
    /// </summary>
    string GetPasswordResetTemplate(
        string resetUrl);

    /// <summary>
    /// Gets the Stripe onboarding email template
    /// </summary>
    string GetOnboardingTemplate(
        string onboardingUrl);
}

