using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Domain.Enums;
using AspireApp.Email.Domain.Events;
using AspireApp.Email.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspireApp.Email.Application.UseCases;

/// <summary>
/// Fast async OTP email use case that queues email for background processing.
/// Returns immediately without waiting for email to be sent.
/// </summary>
public class SendOTPEmailAsyncUseCase : BaseUseCase
{
    private readonly IEmailManager _emailManager;
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SendOTPEmailAsyncUseCase> _logger;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public SendOTPEmailAsyncUseCase(
        IEmailManager emailManager,
        IEmailTemplateProvider templateProvider,
        IEmailLogRepository emailLogRepository,
        IBackgroundTaskQueue backgroundTaskQueue,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SendOTPEmailAsyncUseCase> logger)
        : base(unitOfWork, mapper)
    {
        _emailManager = emailManager;
        _templateProvider = templateProvider;
        _emailLogRepository = emailLogRepository;
        _backgroundTaskQueue = backgroundTaskQueue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _senderEmail = configuration["Email:SenderEmail"] ?? "booking@example.com";
        _senderName = configuration["Email:SenderName"] ?? "Booking";
    }

    /// <summary>
    /// Queues OTP email for background sending and returns immediately.
    /// </summary>
    public async Task<Result<EmailQueuedResponseDto>> ExecuteAsync(
        SendOTPEmailDto dto,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var htmlContent = _templateProvider.GetOtpTemplate(dto.ClubName, dto.Otp);
                var subject = "OTP Confirmation";

                _emailManager.ValidateEmailRequest(dto.Email, subject, htmlContent);

                var priority = _emailManager.GetPriorityForEmailType(EmailType.OTP);

                // Create email log with "Queued" status
                var emailLog = _emailManager.CreateEmailLog(
                    EmailType.OTP,
                    priority,
                    dto.Email,
                    _senderEmail,
                    subject,
                    htmlContent);

                // Save to database first
                await _emailLogRepository.InsertAsync(emailLog, ct);
                await UnitOfWork.SaveChangesAsync(ct);

                var emailLogId = emailLog.Id;
                var toEmail = dto.Email;

                // Queue the actual email sending in the background
                _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
                {
                    try
                    {
                        _logger.LogInformation("Starting background email send for EmailLog ID: {EmailLogId}", emailLogId);

                        // Create a new scope for background processing
                        using var scope = _serviceScopeFactory.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        var emailLogRepository = scope.ServiceProvider.GetRequiredService<IEmailLogRepository>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        // Send email
                        var (success, messageId, error) = await emailService.SendEmailAsync(
                            toEmail,
                            _senderEmail,
                            _senderName,
                            subject,
                            htmlContent,
                            cancellationToken: token);

                        // Update email log status
                        var emailLogToUpdate = await emailLogRepository.GetAsync(emailLogId, cancellationToken: token);
                        
                        if (emailLogToUpdate != null)
                        {
                            if (success)
                            {
                                emailLogToUpdate.MarkAsSent(messageId);
                                emailLogToUpdate.AddDomainEvent(new EmailSentEvent(
                                    emailLogToUpdate.Id, toEmail, subject, DateTime.UtcNow));
                                _logger.LogInformation("Email sent successfully for EmailLog ID: {EmailLogId}", emailLogId);
                            }
                            else
                            {
                                emailLogToUpdate.MarkAsFailed(error ?? "Unknown error");
                                _logger.LogError("Email send failed for EmailLog ID: {EmailLogId}. Error: {Error}", emailLogId, error);
                            }

                            await emailLogRepository.UpdateAsync(emailLogToUpdate, token);
                            await unitOfWork.SaveChangesAsync(token);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background email sending failed for EmailLog ID: {EmailLogId}", emailLogId);
                        
                        // Try to mark as failed
                        try
                        {
                            using var scope = _serviceScopeFactory.CreateScope();
                            var emailLogRepository = scope.ServiceProvider.GetRequiredService<IEmailLogRepository>();
                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                            
                            var emailLogToUpdate = await emailLogRepository.GetAsync(emailLogId, cancellationToken: token);
                            if (emailLogToUpdate != null)
                            {
                                emailLogToUpdate.MarkAsFailed($"Exception: {ex.Message}");
                                await emailLogRepository.UpdateAsync(emailLogToUpdate, token);
                                await unitOfWork.SaveChangesAsync(token);
                            }
                        }
                        catch (Exception updateEx)
                        {
                            _logger.LogError(updateEx, "Failed to update email log status for EmailLog ID: {EmailLogId}", emailLogId);
                        }
                    }
                });

                // Return immediately with queued status
                var response = new EmailQueuedResponseDto
                {
                    Id = emailLogId.ToString(),
                    Email = dto.Email,
                    Status = "Queued",
                    Message = "Email queued for sending. It will be sent shortly.",
                    QueuedAt = DateTime.UtcNow
                };

                _logger.LogInformation("OTP email queued successfully for {Email}. EmailLog ID: {EmailLogId}", dto.Email, emailLogId);

                return Result.Success(response);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Domain error while queuing OTP email for {Email}", dto.Email);
                return Result.Failure<EmailQueuedResponseDto>(ex.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue OTP email for {Email}", dto.Email);
                return Result.Failure<EmailQueuedResponseDto>(
                    DomainErrors.General.InternalError($"Failed to queue OTP email: {ex.Message}"));
            }
        }, cancellationToken);
    }
}

