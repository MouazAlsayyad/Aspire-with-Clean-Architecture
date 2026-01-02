namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending OTP verification email
/// </summary>
public class SendOTPEmailDto
{
    public string ClubName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

