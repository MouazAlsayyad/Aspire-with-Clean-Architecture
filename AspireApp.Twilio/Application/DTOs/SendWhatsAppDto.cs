namespace AspireApp.Twilio.Application.DTOs;

public record SendWhatsAppDto(
    string ToPhoneNumber,
    string? Message = null,
    string? TemplateId = null,
    Dictionary<string, object>? TemplateVariables = null
);

