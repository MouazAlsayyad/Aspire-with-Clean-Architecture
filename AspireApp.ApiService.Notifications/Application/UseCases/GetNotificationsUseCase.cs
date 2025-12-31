using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Notifications.Application.DTOs;
using AspireApp.ApiService.Notifications.Domain.Interfaces;

namespace AspireApp.ApiService.Notifications.Application.UseCases;

public class GetNotificationsUseCase : BaseUseCase
{
    private readonly INotificationManager _notificationManager;

    public GetNotificationsUseCase(
        INotificationManager notificationManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _notificationManager = notificationManager;
    }

    public async Task<Result<GetNotificationsResponseDto>> ExecuteAsync(
        Guid userId,
        GetNotificationsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (notifications, hasMore) = await _notificationManager.GetNotificationsAsync(
                userId,
                request.LastNotificationId,
                request.PageSize,
                request.TimeFilter,
                cancellationToken);

            var notificationDtos = Mapper.Map<List<NotificationDto>>(notifications);
            var lastNotificationId = notifications.LastOrDefault()?.Id;

            return Result.Success(new GetNotificationsResponseDto(
                notificationDtos,
                hasMore,
                lastNotificationId
            ));
        }
        catch (Exception ex)
        {
            return Result.Failure<GetNotificationsResponseDto>(ex.Message);
        }
    }
}

