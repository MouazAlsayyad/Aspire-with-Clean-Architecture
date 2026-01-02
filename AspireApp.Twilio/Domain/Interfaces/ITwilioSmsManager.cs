using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;

namespace AspireApp.Twilio.Domain.Interfaces;

/// <summary>
/// Interface for Twilio SMS/WhatsApp domain service (Manager).
/// Handles messaging-related domain logic and business rules.
/// </summary>
public interface ITwilioSmsManager : IDomainService
{
    /// <summary>
    /// Sends an SMS message
    /// </summary>
    Task<Message> SendSmsAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a WhatsApp message using plain text
    /// </summary>
    Task<Message> SendWhatsAppAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a WhatsApp message using a Twilio Content Template
    /// </summary>
    Task<Message> SendWhatsAppWithTemplateAsync(
        string toPhoneNumber,
        string templateId,
        Dictionary<string, object> templateVariables,
        string? statusCallbackUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates and sends an OTP code via WhatsApp (with SMS fallback)
    /// </summary>
    Task<(Otp Otp, Message Message)> SendOtpAsync(
        string phoneNumber,
        string? name = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an OTP code via WhatsApp template
    /// </summary>
    Task<Message> SendWhatsAppOtpAsync(
        string phoneNumber,
        string name,
        string otp,
        string? statusCallbackUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an OTP code via SMS
    /// </summary>
    Task<Message> SendSmsOtpAsync(
        string phoneNumber,
        string otp,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an OTP code for a phone number
    /// </summary>
    Task<bool> ValidateOtpAsync(
        string phoneNumber,
        string otpCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a new OTP code
    /// </summary>
    Task<Otp> GenerateOtpAsync(
        string phoneNumber,
        int expirationMinutes = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates message status from Twilio webhook
    /// </summary>
    Task UpdateMessageStatusAsync(
        string messageSid,
        MessageStatus status,
        string? failureReason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles WhatsApp message failure and falls back to SMS
    /// </summary>
    Task<Message?> HandleWhatsAppFailureAsync(
        string messageSid,
        string failureReason,
        CancellationToken cancellationToken = default);
}

