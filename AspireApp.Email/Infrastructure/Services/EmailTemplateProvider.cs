using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Infrastructure.Templates;

namespace AspireApp.Email.Infrastructure.Services;

/// <summary>
/// Provides HTML email templates
/// </summary>
public class EmailTemplateProvider : IEmailTemplateProvider
{
    public string GetBookingTemplate(
        string playerName,
        string courtName,
        string tenantName,
        string bookingDate,
        string fromTime,
        string paymentLink)
    {
        return BookingTemplate.GetTemplate(
            playerName, courtName, tenantName, bookingDate, fromTime, paymentLink);
    }

    public string GetCompletedBookingTemplate(
        string tenantName,
        string bookingDate,
        string fromTime,
        double amount)
    {
        return CompletedBookingTemplate.GetTemplate(tenantName, bookingDate, fromTime, amount);
    }

    public string GetMembershipTemplate(
        string playerName,
        string tenantName,
        string membershipDate,
        string paymentLink)
    {
        return MembershipTemplate.GetTemplate(playerName, tenantName, membershipDate, paymentLink);
    }

    public string GetOtpTemplate(string clubName, string otp)
    {
        return OTPTemplate.GetTemplate(clubName, otp);
    }

    public string GetPayoutOtpTemplate(string clubName, string otp)
    {
        return PayoutOTPTemplate.GetTemplate(clubName, otp);
    }

    public string GetPayoutConfirmationTemplate(string tenantName, double amount)
    {
        return PayoutConfirmationTemplate.GetTemplate(tenantName, amount);
    }

    public string GetPayoutRejectionTemplate(string tenantName)
    {
        return PayoutRejectionTemplate.GetTemplate(tenantName);
    }

    public string GetSubscriptionTemplate(
        string tenantName,
        string subscriptionType,
        string length)
    {
        return SubscriptionTemplate.GetTemplate(tenantName, subscriptionType, length);
    }

    public string GetPasswordResetTemplate(string resetUrl)
    {
        return PasswordResetTemplate.GetTemplate(resetUrl);
    }

    public string GetOnboardingTemplate(string onboardingUrl)
    {
        return OnboardingTemplate.GetTemplate(onboardingUrl);
    }
}

