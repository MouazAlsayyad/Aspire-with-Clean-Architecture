namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for sending completed booking notification email (for tenants)
/// </summary>
public class SendCompletedBookingEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string BookingDate { get; set; } = string.Empty;
    public string TimeStr { get; set; } = string.Empty;
    public double Amount { get; set; }
}

