using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.Notifications.UseCases;

public class MarkAllNotificationsAsReadUseCase : BaseUseCase
{
    private readonly Domain.Notifications.Interfaces.INotificationManager _notificationManager;

    public MarkAllNotificationsAsReadUseCase(
        Domain.Notifications.Interfaces.INotificationManager notificationManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _notificationManager = notificationManager;
    }

    public async Task<Result<int>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var count = await _notificationManager.MarkAllAsReadAsync(userId, ct);
                return Result.Success(count);
            }
            catch (Exception ex)
            {
                return Result.Failure<int>(DomainErrors.General.ServerError(ex.Message));
            }
        }, cancellationToken);
    }
}

