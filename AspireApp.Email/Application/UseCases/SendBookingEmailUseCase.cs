using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Domain.Enums;
using AspireApp.Email.Domain.Events;
using AspireApp.Email.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace AspireApp.Email.Application.UseCases;

public class SendBookingEmailUseCase : BaseUseCase
{
    private readonly IEmailManager _emailManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IResiliencePolicy _resiliencePolicy;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public SendBookingEmailUseCase(
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
        _senderEmail = configuration["Email:SenderEmail"] ?? "booking@Sender.com";
        _senderName = configuration["Email:SenderName"] ?? "Sender";
    }

    public async Task<Result<EmailLogDto>> ExecuteAsync(
        SendBookingEmailDto dto,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                // Get HTML template
                var htmlContent = _templateProvider.GetBookingTemplate(
                    dto.PlayerName,
                    dto.CourtName,
                    dto.BookingDate,
                    dto.TimeStr,
                    dto.PaymentLink);

                var subject = "New Booking Confirmation";

                // Validate email request
                _emailManager.ValidateEmailRequest(dto.Email, subject, htmlContent);

                // Get priority
                var priority = _emailManager.GetPriorityForEmailType(EmailType.Booking);

                // Create email log
                var emailLog = _emailManager.CreateEmailLog(
                    EmailType.Booking,
                    priority,
                    dto.Email,
                    _senderEmail,
                    subject,
                    htmlContent);

                await _emailLogRepository.InsertAsync(emailLog, ct);

                // Send email via SendGrid
                var (success, messageId, error) = await _resiliencePolicy.ExecuteAsync(
                    async () => await _emailService.SendEmailAsync(
                        dto.Email,
                        _senderEmail,
                        _senderName,
                        subject,
                        htmlContent,
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
                    DomainErrors.General.InternalError($"Failed to send booking email: {ex.Message}"));
            }
        }, cancellationToken);
    }
}

