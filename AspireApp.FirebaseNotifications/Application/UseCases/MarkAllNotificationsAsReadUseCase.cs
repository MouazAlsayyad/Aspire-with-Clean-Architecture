using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Interfaces;

namespace AspireApp.FirebaseNotifications.Application.UseCases;

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
                return Result.Failure<int>(Error.Failure("Notification.MarkAllReadFailed", ex.Message));
            }
        }, cancellationToken);
    }
}

