namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending Stripe onboarding email
/// </summary>
public class SendOnboardingEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string OnboardingUrl { get; set; } = string.Empty;
}

