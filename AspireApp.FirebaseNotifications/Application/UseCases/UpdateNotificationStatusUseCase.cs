using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Interfaces;

namespace AspireApp.FirebaseNotifications.Application.UseCases;

public class UpdateNotificationStatusUseCase : BaseUseCase
{
    private readonly INotificationManager _notificationManager;

    public UpdateNotificationStatusUseCase(
        INotificationManager notificationManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _notificationManager = notificationManager;
    }

    public async Task<Result> ExecuteAsync(Guid notificationId, bool isRead, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                await _notificationManager.UpdateNotificationStatusAsync(notificationId, isRead, ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Failure("Notification.UpdateFailed", ex.Message));
            }
        }, cancellationToken);
    }
}

