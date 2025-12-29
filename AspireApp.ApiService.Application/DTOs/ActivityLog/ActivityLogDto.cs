using AspireApp.ApiService.Domain.Enums;

namespace AspireApp.ApiService.Application.DTOs.ActivityLog;

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

