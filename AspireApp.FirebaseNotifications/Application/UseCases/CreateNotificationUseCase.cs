using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Application.DTOs;
using AspireApp.FirebaseNotifications.Domain.Interfaces;

namespace AspireApp.FirebaseNotifications.Application.UseCases;

public class CreateNotificationUseCase : BaseUseCase
{
    private readonly INotificationManager _notificationManager;

    public CreateNotificationUseCase(
        INotificationManager notificationManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _notificationManager = notificationManager;
    }

    public async Task<Result> ExecuteAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                await _notificationManager.CreateNotificationAsync(
                    dto.Type,
                    dto.Priority,
                    dto.Title,
                    dto.TitleAr,
                    dto.Message,
                    dto.MessageAr,
                    dto.UserId,
                    dto.ActionUrl,
                    ct);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Failure("Notification.CreateFailed", ex.Message));
            }
        }, cancellationToken);
    }
}

