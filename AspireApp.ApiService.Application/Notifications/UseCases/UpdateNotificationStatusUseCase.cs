using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.Notifications.UseCases;

public class UpdateNotificationStatusUseCase : BaseUseCase
{
    private readonly Domain.Notifications.Interfaces.INotificationManager _notificationManager;

    public UpdateNotificationStatusUseCase(
        Domain.Notifications.Interfaces.INotificationManager notificationManager,
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
                return Result.Failure(DomainErrors.General.ServerError(ex.Message));
            }
        }, cancellationToken);
    }
}

