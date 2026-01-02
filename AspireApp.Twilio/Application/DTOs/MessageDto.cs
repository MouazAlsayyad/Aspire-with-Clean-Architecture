using AspireApp.Twilio.Domain.Enums;

namespace AspireApp.Twilio.Application.DTOs;

public record MessageDto(
    Guid Id,
    string RecipientPhoneNumber,
    string MessageBody,
    MessageChannel Channel,
    MessageStatus Status,
    string? MessageSid,
    DateTime? SentAt,
    DateTime? DeliveredAt,
    DateTime? FailedAt,
    string? FailureReason,
    string? TemplateId,
    string? TemplateVariables,
    DateTime CreationTime
);

