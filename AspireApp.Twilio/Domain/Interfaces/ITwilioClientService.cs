using AspireApp.Domain.Shared.Interfaces;

namespace AspireApp.Twilio.Domain.Interfaces;

/// <summary>
/// Interface for Twilio API client service.
/// Handles direct communication with Twilio API.
/// </summary>
public interface ITwilioClientService : IDomainService
{
    /// <summary>
    /// Sends an SMS message via Twilio
    /// </summary>
    /// <returns>Twilio Message SID if successful, null otherwise</returns>
    Task<string?> SendSmsAsync(
        string toPhoneNumber,
        string fromPhoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a WhatsApp message via Twilio (plain text)
    /// </summary>
    /// <returns>Twilio Message SID if successful, null otherwise</returns>
    Task<string?> SendWhatsAppAsync(
        string toPhoneNumber,
        string fromWhatsAppNumber,
        string message,
        string? statusCallbackUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a WhatsApp message using Twilio Content Template
    /// </summary>
    /// <returns>Twilio Message SID if successful, null otherwise</returns>
    Task<string?> SendWhatsAppWithTemplateAsync(
        string toPhoneNumber,
        string fromWhatsAppNumber,
        string templateId,
        Dictionary<string, object> templateVariables,
        string? statusCallbackUrl = null,
        CancellationToken cancellationToken = default);
}

