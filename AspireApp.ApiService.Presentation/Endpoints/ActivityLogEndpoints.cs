using AspireApp.ApiService.Application.ActivityLogs.DTOs;
using AspireApp.ApiService.Application.UseCases.ActivityLogs;
using AspireApp.ApiService.Domain.ActivityLogs.Enums;
using AspireApp.ApiService.Domain.Permissions;
using AspireApp.ApiService.Presentation.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Endpoints;

public static class ActivityLogEndpoints
{
    public static void MapActivityLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activity-logs")
            .WithTags("ActivityLogs")
            .WithValidation();

        group.MapGet("/", GetActivityLogs)
            .WithName("GetActivityLogs")
            .WithSummary("Get activity logs with filtering, search, and pagination")
            .WithDescription("Retrieves a paginated list of activity logs. Supports filtering by activity type, user, entity, severity, date range, and keyword search.")
            .Produces<PagedActivityLogsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(PermissionNames.ActivityLog.Read);
    }

    private static async Task<IResult> GetActivityLogs(
        [FromServices] GetActivityLogsUseCase useCase,
        CancellationToken cancellationToken,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchKeyword = null,
        [FromQuery] string? activityType = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] ActivitySeverity? severity = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool? isPublic = null)
    {
        var request = new GetActivityLogsRequest(
            PageNumber: pageNumber,
            PageSize: pageSize,
            SearchKeyword: searchKeyword,
            ActivityType: activityType,
            UserId: userId,
            EntityId: entityId,
            EntityType: entityType,
            Severity: severity,
            StartDate: startDate,
            EndDate: endDate,
            IsPublic: isPublic
        );

        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.ToHttpResult();
    }
}

