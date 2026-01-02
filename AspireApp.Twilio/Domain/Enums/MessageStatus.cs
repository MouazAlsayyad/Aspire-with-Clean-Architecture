namespace AspireApp.Twilio.Domain.Enums;

/// <summary>
/// Represents the status of a message in the delivery pipeline.
/// </summary>
public enum MessageStatus
{
    /// <summary>
    /// Message is queued for sending
    /// </summary>
    Queued = 1,

    /// <summary>
    /// Message has been sent to the provider
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Message has been delivered to the recipient
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// Message delivery failed
    /// </summary>
    Failed = 4
}

