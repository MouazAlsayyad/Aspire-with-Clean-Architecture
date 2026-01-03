using AspireApp.ApiService.Domain.ActivityLogs.Enums;

namespace AspireApp.ApiService.Application.ActivityLogs.DTOs;

public record GetActivityLogsRequest(
    int PageNumber = 1,
    int PageSize = 50,
    string? SearchKeyword = null,
    string? ActivityType = null,
    Guid? UserId = null,
    Guid? EntityId = null,
    string? EntityType = null,
    ActivitySeverity? Severity = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    bool? IsPublic = null
);

