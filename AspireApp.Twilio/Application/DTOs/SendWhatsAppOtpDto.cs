namespace AspireApp.Twilio.Application.DTOs;

public record SendWhatsAppOtpDto(
    string PhoneNumber,
    string Name,
    string Otp
);

