using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Notifications.DTOs;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Notifications.Interfaces;

namespace AspireApp.ApiService.Application.Notifications.UseCases;

public class GetNotificationsUseCase : BaseUseCase
{
    private readonly Domain.Notifications.Interfaces.INotificationManager _notificationManager;

    public GetNotificationsUseCase(
        Domain.Notifications.Interfaces.INotificationManager notificationManager,
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

            var notificationDtos = Mapper.Map<List<DTOs.NotificationDto>>(notifications);
            var lastNotificationId = notifications.LastOrDefault()?.Id;

            return Result.Success(new GetNotificationsResponseDto(
                notificationDtos,
                hasMore,
                lastNotificationId
            ));
        }
        catch (Exception ex)
        {
            return Result.Failure<GetNotificationsResponseDto>(DomainErrors.General.ServerError(ex.Message));
        }
    }
}

