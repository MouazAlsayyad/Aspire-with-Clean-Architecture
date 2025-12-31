using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Notifications.Domain.Interfaces;

namespace AspireApp.ApiService.Notifications.Application.UseCases;

public class MarkAllNotificationsAsReadUseCase : BaseUseCase
{
    private readonly INotificationManager _notificationManager;

    public MarkAllNotificationsAsReadUseCase(
        INotificationManager notificationManager,
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
                return Result.Failure<int>(ex.Message);
            }
        }, cancellationToken);
    }
}

