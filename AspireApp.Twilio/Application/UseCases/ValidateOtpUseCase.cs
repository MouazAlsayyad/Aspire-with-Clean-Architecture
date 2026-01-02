using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Domain.Interfaces;

namespace AspireApp.Twilio.Application.UseCases;

public class ValidateOtpUseCase : BaseUseCase
{
    private readonly ITwilioSmsManager _twilioSmsManager;

    public ValidateOtpUseCase(
        ITwilioSmsManager twilioSmsManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _twilioSmsManager = twilioSmsManager;
    }

    public async Task<Result<bool>> ExecuteAsync(ValidateOtpDto dto, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var isValid = await _twilioSmsManager.ValidateOtpAsync(
                    dto.PhoneNumber,
                    dto.OtpCode,
                    ct);

                return Result.Success(isValid);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>(DomainErrors.General.ServerError(ex.Message));
            }
        }, cancellationToken);
    }
}

