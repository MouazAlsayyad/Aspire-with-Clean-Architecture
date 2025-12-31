using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Notifications.DTOs;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Notifications.Interfaces;

namespace AspireApp.ApiService.Application.Notifications.UseCases;

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
                return Result.Failure(DomainErrors.General.ServerError(ex.Message));
            }
        }, cancellationToken);
    }
}

