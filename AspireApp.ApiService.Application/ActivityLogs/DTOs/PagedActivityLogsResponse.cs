using AspireApp.Domain.Shared.Common;

namespace AspireApp.ApiService.Application.ActivityLogs.DTOs;

public record PagedActivityLogsResponse(
    IEnumerable<ActivityLogDto> Items,
    PaginationInfo Pagination
);

