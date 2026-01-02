using AspireApp.Modules.ActivityLogs.Domain.Enums;

namespace AspireApp.Modules.ActivityLogs.Application.DTOs;

public record ActivityLogDto(
    Guid Id,
    string ActivityType,
    string DescriptionTemplate,
    string? DescriptionParameters,
    Guid? UserId,
    string? UserName,
    Guid? EntityId,
    string? EntityType,
    string? Metadata,
    string? IpAddress,
    string? UserAgent,
    ActivitySeverity Severity,
    bool IsPublic,
    string? Tags,
    DateTime CreationTime
);

