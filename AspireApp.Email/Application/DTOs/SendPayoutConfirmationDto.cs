namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending payout confirmation email
/// </summary>
public class SendPayoutConfirmationDto
{
    public string Email { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? CsvContent { get; set; }
    public string? PdfBase64 { get; set; }
}

