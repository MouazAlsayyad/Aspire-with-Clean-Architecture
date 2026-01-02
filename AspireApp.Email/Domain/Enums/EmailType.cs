namespace AspireApp.Email.Domain.Enums;

/// <summary>
/// Represents the type of email being sent
/// </summary>
public enum EmailType
{
    /// <summary>
    /// New booking confirmation email
    /// </summary>
    Booking = 1,

    /// <summary>
    /// Completed booking notification (for tenants)
    /// </summary>
    CompletedBooking = 2,

    /// <summary>
    /// Membership subscription email
    /// </summary>
    Membership = 3,

    /// <summary>
    /// OTP verification email
    /// </summary>
    OTP = 4,

    /// <summary>
    /// Payout OTP verification email
    /// </summary>
    PayoutOTP = 5,

    /// <summary>
    /// Payout confirmation email
    /// </summary>
    PayoutConfirmation = 6,

    /// <summary>
    /// Payout rejection email
    /// </summary>
    PayoutRejection = 7,

    /// <summary>
    /// Subscription invoice email
    /// </summary>
    Subscription = 8,

    /// <summary>
    /// Password reset email
    /// </summary>
    ForgotPassword = 9,

    /// <summary>
    /// Stripe onboarding email
    /// </summary>
    Onboarding = 10
}

