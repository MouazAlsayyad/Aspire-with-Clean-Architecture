namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending subscription invoice email
/// </summary>
public class SendSubscriptionEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string SubscriptionType { get; set; } = string.Empty;
    public string Length { get; set; } = string.Empty;
    public string? InvoicePdfBase64 { get; set; }
}

