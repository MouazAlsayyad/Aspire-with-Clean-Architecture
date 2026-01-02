using AspireApp.Modules.ActivityLogs.Domain.Enums;

namespace AspireApp.Modules.ActivityLogs.Domain.Interfaces;

/// <summary>
/// Interface for logging activities in the system
/// </summary>
public interface IActivityLogger
{
    /// <summary>
    /// Logs an activity with template-based description
    /// </summary>
    Task LogAsync(
        string activityType,
        string descriptionTemplateKey,
        Dictionary<string, object>? descriptionParameters = null,
        Guid? entityId = null,
        string? entityType = null,
        Dictionary<string, object>? metadata = null,
        ActivitySeverity? severity = null,
        bool? isPublic = null,
        params string[] tags);

    /// <summary>
    /// Logs an activity with a simple description string
    /// </summary>
    Task LogAsync(
        string activityType,
        string description,
        Guid? entityId = null,
        string? entityType = null,
        Dictionary<string, object>? metadata = null,
        ActivitySeverity? severity = null,
        bool? isPublic = null,
        params string[] tags);

    /// <summary>
    /// Logs an activity for a strongly-typed entity
    /// </summary>
    Task LogAsync<TEntity>(
        string activityType,
        string descriptionTemplateKey,
        Guid entityId,
        Dictionary<string, object>? descriptionParameters = null,
        Dictionary<string, object>? metadata = null,
        ActivitySeverity? severity = null,
        bool? isPublic = null,
        params string[] tags) where TEntity : class;
}

