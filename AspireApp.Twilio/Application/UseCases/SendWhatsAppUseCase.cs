using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Domain.Interfaces;

namespace AspireApp.Twilio.Application.UseCases;

public class SendWhatsAppUseCase : BaseUseCase
{
    private readonly ITwilioSmsManager _twilioSmsManager;

    public SendWhatsAppUseCase(
        ITwilioSmsManager twilioSmsManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _twilioSmsManager = twilioSmsManager;
    }

    public async Task<Result<MessageDto>> ExecuteAsync(SendWhatsAppDto dto, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var message = dto.TemplateId != null
                    ? await _twilioSmsManager.SendWhatsAppWithTemplateAsync(
                        dto.ToPhoneNumber,
                        dto.TemplateId,
                        dto.TemplateVariables ?? new Dictionary<string, object>(),
                        null,
                        ct)
                    : await _twilioSmsManager.SendWhatsAppAsync(
                        dto.ToPhoneNumber,
                        dto.Message ?? string.Empty,
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

