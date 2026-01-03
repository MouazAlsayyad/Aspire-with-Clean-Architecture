namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending booking confirmation email
/// </summary>
public class SendBookingEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string BookingDate { get; set; } = string.Empty;
    public string TimeStr { get; set; } = string.Empty;
    public string PaymentLink { get; set; } = string.Empty;
}

