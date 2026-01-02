namespace AspireApp.Email.Domain.Enums;

/// <summary>
/// Represents the priority of an email
/// </summary>
public enum EmailPriority
{
    /// <summary>
    /// Low priority - can be delayed
    /// </summary>
    Low = 1,

    /// <summary>
    /// Normal priority - standard processing
    /// </summary>
    Normal = 2,

    /// <summary>
    /// High priority - process immediately (e.g., OTP, password reset)
    /// </summary>
    High = 3
}

