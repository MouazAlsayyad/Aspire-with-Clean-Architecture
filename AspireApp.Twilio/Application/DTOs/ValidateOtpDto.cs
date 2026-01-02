namespace AspireApp.Twilio.Application.DTOs;

public record ValidateOtpDto(
    string PhoneNumber,
    string OtpCode
);

