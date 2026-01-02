namespace AspireApp.Twilio.Application.DTOs;

public record SendOtpDto(
    string PhoneNumber,
    string? Name = null
);

