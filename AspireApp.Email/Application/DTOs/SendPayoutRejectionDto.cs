namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending payout rejection email
/// </summary>
public class SendPayoutRejectionDto
{
    public string Email { get; set; } = string.Empty;
}

