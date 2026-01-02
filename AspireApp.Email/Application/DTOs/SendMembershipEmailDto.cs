namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending membership subscription email
/// </summary>
public class SendMembershipEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string MembershipDate { get; set; } = string.Empty;
    public string PaymentLink { get; set; } = string.Empty;
    public string? TenantEmail { get; set; }
}

