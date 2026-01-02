namespace AspireApp.Twilio.Application.DTOs;

public record SendSmsDto(
    string ToPhoneNumber,
    string Message
);

