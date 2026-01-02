using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Domain.Enums;
using AspireApp.Email.Domain.Events;
using AspireApp.Email.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace AspireApp.Email.Application.UseCases;

public class SendCompletedBookingEmailUseCase : BaseUseCase
{
    private readonly IEmailManager _emailManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public SendCompletedBookingEmailUseCase(
        IEmailManager emailManager,
        IEmailService emailService,
        IEmailTemplateProvider templateProvider,
        IEmailLogRepository emailLogRepository,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _emailManager = emailManager;
        _emailService = emailService;
        _templateProvider = templateProvider;
        _emailLogRepository = emailLogRepository;
        _senderEmail = configuration["Email:SenderEmail"] ?? "booking@example.com";
        _senderName = configuration["Email:SenderName"] ?? "Booking";
    }

    public async Task<Result<EmailLogDto>> ExecuteAsync(
        SendCompletedBookingEmailDto dto,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var htmlContent = _templateProvider.GetCompletedBookingTemplate(
                    dto.TenantName,
                    dto.BookingDate,
                    dto.TimeStr,
                    dto.Amount);

                var subject = "Completed Booking";

                _emailManager.ValidateEmailRequest(dto.Email, subject, htmlContent);

                var priority = _emailManager.GetPriorityForEmailType(EmailType.CompletedBooking);

                var emailLog = _emailManager.CreateEmailLog(
                    EmailType.CompletedBooking,
                    priority,
                    dto.Email,
                    _senderEmail,
                    subject,
                    htmlContent);

                await _emailLogRepository.InsertAsync(emailLog, ct);

                var (success, messageId, error) = await _emailService.SendEmailAsync(
                    dto.Email,
                    _senderEmail,
                    _senderName,
                    subject,
                    htmlContent,
                    cancellationToken: ct);

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
                    DomainErrors.General.InternalError($"Failed to send completed booking email: {ex.Message}"));
            }
        }, cancellationToken);
    }
}

