namespace AspireApp.Twilio.Domain.Enums;

/// <summary>
/// Represents the channel through which a message is sent.
/// </summary>
public enum MessageChannel
{
    /// <summary>
    /// Message sent via WhatsApp
    /// </summary>
    WhatsApp = 1,

    /// <summary>
    /// Message sent via SMS
    /// </summary>
    SMS = 2
}

