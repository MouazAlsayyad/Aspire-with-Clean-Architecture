using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Infrastructure.Templates;

namespace AspireApp.Email.Infrastructure.Services;

/// <summary>
/// Provides HTML email templates using strategy pattern
/// </summary>
public class EmailTemplateProvider : IEmailTemplateProvider
{
    private readonly IBookingEmailTemplateStrategy _bookingStrategy;
    private readonly ICompletedBookingEmailTemplateStrategy _completedBookingStrategy;
    private readonly IMembershipEmailTemplateStrategy _membershipStrategy;
    private readonly IPayoutConfirmationEmailTemplateStrategy _payoutConfirmationStrategy;
    private readonly IPayoutRejectionEmailTemplateStrategy _payoutRejectionStrategy;
    private readonly ISubscriptionEmailTemplateStrategy _subscriptionStrategy;

    public EmailTemplateProvider(
        IBookingEmailTemplateStrategy bookingStrategy,
        ICompletedBookingEmailTemplateStrategy completedBookingStrategy,
        IMembershipEmailTemplateStrategy membershipStrategy,
        IPayoutConfirmationEmailTemplateStrategy payoutConfirmationStrategy,
        IPayoutRejectionEmailTemplateStrategy payoutRejectionStrategy,
        ISubscriptionEmailTemplateStrategy subscriptionStrategy)
    {
        _bookingStrategy = bookingStrategy;
        _completedBookingStrategy = completedBookingStrategy;
        _membershipStrategy = membershipStrategy;
        _payoutConfirmationStrategy = payoutConfirmationStrategy;
        _payoutRejectionStrategy = payoutRejectionStrategy;
        _subscriptionStrategy = subscriptionStrategy;
    }

    public string GetBookingTemplate(
        string playerName,
        string courtName,
        string bookingDate,
        string fromTime,
        string paymentLink)
    {
        return _bookingStrategy.GetTemplate(playerName, courtName, bookingDate, fromTime, paymentLink);
    }

    public string GetCompletedBookingTemplate(
        string bookingDate,
        string fromTime,
        double amount)
    {
        return _completedBookingStrategy.GetTemplate(bookingDate, fromTime, amount);
    }

    public string GetMembershipTemplate(
        string playerName,
        string membershipDate,
        string paymentLink)
    {
        return _membershipStrategy.GetTemplate(playerName, membershipDate, paymentLink);
    }

    public string GetOtpTemplate(string clubName, string otp)
    {
        return OTPTemplate.GetTemplate(clubName, otp);
    }

    public string GetPayoutOtpTemplate(string clubName, string otp)
    {
        return PayoutOTPTemplate.GetTemplate(clubName, otp);
    }

    public string GetPayoutConfirmationTemplate(double amount)
    {
        return _payoutConfirmationStrategy.GetTemplate(amount);
    }

    public string GetPayoutRejectionTemplate()
    {
        return _payoutRejectionStrategy.GetTemplate();
    }

    public string GetSubscriptionTemplate(
        string subscriptionType,
        string length)
    {
        return _subscriptionStrategy.GetTemplate(subscriptionType, length);
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

