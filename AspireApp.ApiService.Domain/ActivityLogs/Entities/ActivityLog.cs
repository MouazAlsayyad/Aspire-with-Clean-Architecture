using AspireApp.ApiService.Domain.ActivityLogs.Enums;
using AspireApp.Domain.Shared.Entities;

namespace AspireApp.ApiService.Domain.ActivityLogs.Entities;

/// <summary>
/// Represents a logged activity in the system
/// </summary>
public class ActivityLog : BaseEntity
{
    /// <summary>
    /// Type/category of activity (e.g., "OrderCreated", "UserUpdated", "HttpRequest")
    /// </summary>
    public string ActivityType { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable description template
    /// </summary>
    public string DescriptionTemplate { get; private set; } = string.Empty;

    /// <summary>
    /// JSON-serialized parameters for template substitution
    /// </summary>
    public string? DescriptionParameters { get; private set; }

    /// <summary>
    /// ID of the user who performed the activity
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Username for quick reference
    /// </summary>
    public string? UserName { get; private set; }

    /// <summary>
    /// Optional ID of the affected entity
    /// </summary>
    public Guid? EntityId { get; private set; }

    /// <summary>
    /// Optional type of the affected entity
    /// </summary>
    public string? EntityType { get; private set; }

    /// <summary>
    /// Additional JSON metadata
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Client IP address
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Browser/client user agent
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Severity level of the activity
    /// </summary>
    public ActivitySeverity Severity { get; private set; } = ActivitySeverity.Info;

    /// <summary>
    /// Whether this log is visible to end users
    /// </summary>
    public bool IsPublic { get; private set; } = true;

    /// <summary>
    /// Comma-separated tags for categorization
    /// </summary>
    public string? Tags { get; private set; }

    // Private constructor for EF Core
    private ActivityLog() { }

    /// <summary>
    /// Creates a new activity log entry
    /// </summary>
    public ActivityLog(
        string activityType,
        string descriptionTemplate,
        Guid? userId = null,
        string? userName = null,
        Guid? entityId = null,
        string? entityType = null,
        string? descriptionParameters = null,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null,
        ActivitySeverity severity = ActivitySeverity.Info,
        bool isPublic = true,
        string? tags = null)
    {
        ActivityType = activityType ?? throw new ArgumentNullException(nameof(activityType));
        DescriptionTemplate = descriptionTemplate ?? throw new ArgumentNullException(nameof(descriptionTemplate));
        UserId = userId;
        UserName = userName;
        EntityId = entityId;
        EntityType = entityType;
        DescriptionParameters = descriptionParameters;
        Metadata = metadata;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Severity = severity;
        IsPublic = isPublic;
        Tags = tags;
    }
}

