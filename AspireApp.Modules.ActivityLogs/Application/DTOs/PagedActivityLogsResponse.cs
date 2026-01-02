using AspireApp.Domain.Shared.Common;

namespace AspireApp.Modules.ActivityLogs.Application.DTOs;

public record PagedActivityLogsResponse(
    IEnumerable<ActivityLogDto> Items,
    PaginationInfo Pagination
);

