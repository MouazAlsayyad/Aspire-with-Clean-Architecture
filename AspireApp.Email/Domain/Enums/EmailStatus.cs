namespace AspireApp.Email.Domain.Enums;

/// <summary>
/// Represents the status of an email
/// </summary>
public enum EmailStatus
{
    /// <summary>
    /// Email is pending to be sent
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Email is queued for sending
    /// </summary>
    Queued = 2,

    /// <summary>
    /// Email was successfully sent
    /// </summary>
    Sent = 3,

    /// <summary>
    /// Email failed to send
    /// </summary>
    Failed = 4
}

