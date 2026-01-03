using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Domain.Enums;
using AspireApp.Email.Domain.Events;
using AspireApp.Email.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AspireApp.Email.Application.UseCases;

public class SendPayoutConfirmationUseCase : BaseUseCase
{
    private readonly IEmailManager _emailManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string? _adminEmail;
    private readonly bool _enableBcc;

    public SendPayoutConfirmationUseCase(
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
        _adminEmail = configuration["Email:AdminEmail"];
        _enableBcc = configuration.GetValue<bool>("Email:EnableBcc", true);
    }

    public async Task<Result<EmailLogDto>> ExecuteAsync(
        SendPayoutConfirmationDto dto,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var htmlContent = _templateProvider.GetPayoutConfirmationTemplate(
                    dto.Amount);

                var subject = "Payout Confirmation";

                _emailManager.ValidateEmailRequest(dto.Email, subject, htmlContent);

                // Validate attachments if provided
                _emailManager.ValidateAttachment(dto.PdfBase64, "Invoice.pdf");

                var priority = _emailManager.GetPriorityForEmailType(EmailType.PayoutConfirmation);

                // Prepare BCC list
                var bccList = new List<string>();
                if (_enableBcc && !string.IsNullOrEmpty(_adminEmail))
                {
                    bccList.Add(_adminEmail);
                }

                // Prepare attachments
                var attachments = new List<EmailAttachment>();
                
                if (!string.IsNullOrEmpty(dto.PdfBase64))
                {
                    attachments.Add(new EmailAttachment
                    {
                        FileName = "Invoice.pdf",
                        ContentBase64 = dto.PdfBase64,
                        ContentType = "application/pdf",
                        Disposition = "attachment"
                    });
                }

                if (!string.IsNullOrEmpty(dto.CsvContent))
                {
                    var csvBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dto.CsvContent));
                    attachments.Add(new EmailAttachment
                    {
                        FileName = "payout_details.csv",
                        ContentBase64 = csvBase64,
                        ContentType = "text/csv",
                        Disposition = "attachment"
                    });
                }

                var emailLog = _emailManager.CreateEmailLog(
                    EmailType.PayoutConfirmation,
                    priority,
                    dto.Email,
                    _senderEmail,
                    subject,
                    htmlContent,
                    hasAttachments: attachments.Any(),
                    bccAddresses: string.Join(", ", bccList));

                await _emailLogRepository.InsertAsync(emailLog, ct);

                var (success, messageId, error) = await _emailService.SendEmailAsync(
                    dto.Email,
                    _senderEmail,
                    _senderName,
                    subject,
                    htmlContent,
                    attachments: attachments.Any() ? attachments : null,
                    bccAddresses: bccList.Any() ? bccList : null,
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
                    DomainErrors.General.InternalError($"Failed to send payout confirmation email: {ex.Message}"));
            }
        }, cancellationToken);
    }
}

