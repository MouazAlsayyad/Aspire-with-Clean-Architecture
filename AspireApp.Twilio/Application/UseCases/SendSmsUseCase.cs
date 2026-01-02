using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Domain.Interfaces;

namespace AspireApp.Twilio.Application.UseCases;

public class SendSmsUseCase : BaseUseCase
{
    private readonly ITwilioSmsManager _twilioSmsManager;

    public SendSmsUseCase(
        ITwilioSmsManager twilioSmsManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _twilioSmsManager = twilioSmsManager;
    }

    public async Task<Result<MessageDto>> ExecuteAsync(SendSmsDto dto, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var message = await _twilioSmsManager.SendSmsAsync(
                    dto.ToPhoneNumber,
                    dto.Message,
                    ct);

                return Result.Success(Mapper.Map<MessageDto>(message));
            }
            catch (Exception ex)
            {
                return Result.Failure<MessageDto>(DomainErrors.General.ServerError(ex.Message));
            }
        }, cancellationToken);
    }
}

