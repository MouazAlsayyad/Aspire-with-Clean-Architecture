namespace AspireApp.ApiService.Domain.Enums;

/// <summary>
/// Defines severity levels for activity logs
/// </summary>
public enum ActivitySeverity
{
    /// <summary>
    /// Informational activity (default)
    /// </summary>
    Info = 0,

    /// <summary>
    /// Low importance activity
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium importance activity
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High importance activity
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical activity requiring attention
    /// </summary>
    Critical = 4
}

