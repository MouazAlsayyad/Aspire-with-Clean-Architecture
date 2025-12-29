using AspireApp.ApiService.Domain.Common;

namespace AspireApp.ApiService.Application.DTOs.ActivityLog;

public record PagedActivityLogsResponse(
    IEnumerable<ActivityLogDto> Items,
    PaginationInfo Pagination
);

