using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Domain.Interfaces;

namespace AspireApp.Twilio.Application.UseCases;

public class SendOtpUseCase : BaseUseCase
{
    private readonly ITwilioSmsManager _twilioSmsManager;

    public SendOtpUseCase(
        ITwilioSmsManager twilioSmsManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _twilioSmsManager = twilioSmsManager;
    }

    public async Task<Result<MessageDto>> ExecuteAsync(SendOtpDto dto, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var (otp, message) = await _twilioSmsManager.SendOtpAsync(
                    dto.PhoneNumber,
                    dto.Name,
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

