using AspireApp.Twilio.Domain.Enums;

namespace AspireApp.Twilio.Application.DTOs;

public record GetMessagesRequestDto(
    string? PhoneNumber = null,
    MessageChannel? Channel = null,
    MessageStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 50
);

