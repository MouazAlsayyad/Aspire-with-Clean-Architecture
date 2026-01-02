using AspireApp.Email.Domain.Enums;

namespace AspireApp.Email.Application.DTOs;

/// <summary>
/// DTO for getting paginated email logs
/// </summary>
public class GetEmailLogsRequestDto
{
    public EmailType? EmailType { get; set; }
    public EmailStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}

