namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending password reset email
/// </summary>
public class SendPasswordResetDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetUrl { get; set; } = string.Empty;
}

