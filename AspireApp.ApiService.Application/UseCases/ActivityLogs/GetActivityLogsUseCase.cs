using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.ActivityLog;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.ActivityLogs;

public class GetActivityLogsUseCase : BaseUseCase
{
    private readonly IActivityLogStore _activityLogStore;

    public GetActivityLogsUseCase(
        IActivityLogStore activityLogStore,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _activityLogStore = activityLogStore;
    }

    public async Task<Result<PagedActivityLogsResponse>> ExecuteAsync(
        GetActivityLogsRequest request,
        CancellationToken cancellationToken = default)
    {
        // Normalize pagination parameters
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;
        
        // Limit page size to prevent performance issues
        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var (items, totalCount) = await _activityLogStore.GetListAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            activityType: request.ActivityType,
            userId: request.UserId,
            entityId: request.EntityId,
            entityType: request.EntityType,
            severity: request.Severity,
            startDate: request.StartDate,
            endDate: request.EndDate,
            isPublic: request.IsPublic,
            searchTerm: request.SearchKeyword,
            cancellationToken: cancellationToken);

        var activityLogDtos = Mapper.Map<IEnumerable<ActivityLogDto>>(items);
        
        var pagination = new PaginationInfo(totalCount, pageNumber, pageSize);
        var response = new PagedActivityLogsResponse(activityLogDtos, pagination);

        return Result.Success(response);
    }
}

