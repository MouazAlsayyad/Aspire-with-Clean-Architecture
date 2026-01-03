using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Domain.Enums;
using AspireApp.Email.Domain.Events;
using AspireApp.Email.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace AspireApp.Email.Application.UseCases;

public class SendPayoutRejectionUseCase : BaseUseCase
{
    private readonly IEmailManager _emailManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IResiliencePolicy _resiliencePolicy;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string? _adminEmail;
    private readonly bool _enableBcc;

    public SendPayoutRejectionUseCase(
        IEmailManager emailManager,
        IEmailService emailService,
        IEmailTemplateProvider templateProvider,
        IEmailLogRepository emailLogRepository,
        IResiliencePolicy resiliencePolicy,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _emailManager = emailManager;
        _emailService = emailService;
        _templateProvider = templateProvider;
        _emailLogRepository = emailLogRepository;
        _resiliencePolicy = resiliencePolicy;
        _senderEmail = configuration["Email:SenderEmail"] ?? "booking@example.com";
        _senderName = configuration["Email:SenderName"] ?? "Booking";
        _adminEmail = configuration["Email:AdminEmail"];
        _enableBcc = configuration.GetValue<bool>("Email:EnableBcc", true);
    }

    public async Task<Result<EmailLogDto>> ExecuteAsync(
        SendPayoutRejectionDto dto,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var htmlContent = _templateProvider.GetPayoutRejectionTemplate();
                var subject = "Payout Rejection";

                _emailManager.ValidateEmailRequest(dto.Email, subject, htmlContent);

                var priority = _emailManager.GetPriorityForEmailType(EmailType.PayoutRejection);

                var bccList = new List<string>();
                if (_enableBcc && !string.IsNullOrEmpty(_adminEmail))
                {
                    bccList.Add(_adminEmail);
                }

                var emailLog = _emailManager.CreateEmailLog(
                    EmailType.PayoutRejection,
                    priority,
                    dto.Email,
                    _senderEmail,
                    subject,
                    htmlContent,
                    bccAddresses: string.Join(", ", bccList));

                await _emailLogRepository.InsertAsync(emailLog, ct);

                var (success, messageId, error) = await _resiliencePolicy.ExecuteAsync(
                    async () => await _emailService.SendEmailAsync(
                        dto.Email,
                        _senderEmail,
                        _senderName,
                        subject,
                        htmlContent,
                        bccAddresses: bccList.Any() ? bccList : null,
                        cancellationToken: ct),
                    ct);

                if (success)
                {
                    emailLog.MarkAsSent(messageId);
                    emailLog.AddDomainEvent(new EmailSentEvent(
                        emailLog.Id, dto.Email, subject, DateTime.UtcNow));
                }
                else
                {
                    emailLog.MarkAsFailed(error ?? "Unknown error");
                }

                await _emailLogRepository.UpdateAsync(emailLog, ct);

                var emailDto = Mapper.Map<EmailLogDto>(emailLog);

                return success
                    ? Result.Success(emailDto)
                    : Result.Failure<EmailLogDto>(
                        DomainErrors.General.InternalError($"Failed to send email: {error}"));
            }
            catch (DomainException ex)
            {
                return Result.Failure<EmailLogDto>(ex.Error);
            }
            catch (Exception ex)
            {
                return Result.Failure<EmailLogDto>(
                    DomainErrors.General.InternalError($"Failed to send payout rejection email: {ex.Message}"));
            }
        }, cancellationToken);
    }
}

