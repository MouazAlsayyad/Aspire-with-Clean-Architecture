using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Application.UseCases;
using AspireApp.Email.Domain.Entities;
using AspireApp.Email.Domain.Enums;
using AspireApp.Email.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace AspireApp.Email.Tests.Application.UseCases;

public class SendBookingEmailUseCaseTests
{
    private readonly IEmailManager _emailManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IResiliencePolicy _resiliencePolicy;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly SendBookingEmailUseCase _sut;

    public SendBookingEmailUseCaseTests()
    {
        _emailManager = Substitute.For<IEmailManager>();
        _emailService = Substitute.For<IEmailService>();
        _templateProvider = Substitute.For<IEmailTemplateProvider>();
        _emailLogRepository = Substitute.For<IEmailLogRepository>();
        _resiliencePolicy = Substitute.For<IResiliencePolicy>();
        _configuration = Substitute.For<IConfiguration>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        // Mock configuration
        _configuration["Email:SenderEmail"].Returns("booking@sender.com");
        _configuration["Email:SenderName"].Returns("Sender");

        _sut = new SendBookingEmailUseCase(
            _emailManager,
            _emailService,
            _templateProvider,
            _emailLogRepository,
            _resiliencePolicy,
            _configuration,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenEmailIsSentSuccessfully()
    {
        // Arrange
        var sendBookingEmailDto = new SendBookingEmailDto
        {
            Email = "test@example.com",
            PlayerName = "Test Player",
            CourtName = "Test Court",
            BookingDate = "2026-01-15",
            TimeStr = "10:00 AM",
            PaymentLink = "http://example.com/payment"
        };

        var emailLog = new EmailLog(EmailType.Booking, EmailStatus.Queued, EmailPriority.High, "test@example.com", "booking@sender.com", "New Booking Confirmation", "htmlContent");

        _templateProvider.GetBookingTemplate(
            sendBookingEmailDto.PlayerName,
            sendBookingEmailDto.CourtName,
            sendBookingEmailDto.BookingDate,
            sendBookingEmailDto.TimeStr,
            sendBookingEmailDto.PaymentLink).Returns("htmlContent");

        _emailManager.GetPriorityForEmailType(EmailType.Booking).Returns(EmailPriority.High);
        _emailManager.CreateEmailLog(
            EmailType.Booking,
            EmailPriority.High,
            sendBookingEmailDto.Email,
            "booking@sender.com",
            "New Booking Confirmation",
            "htmlContent").Returns(emailLog);

        _emailService.SendEmailAsync(
            sendBookingEmailDto.Email,
            "booking@sender.com",
            "Sender",
            "New Booking Confirmation",
            "htmlContent",
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns((true, "messageId", string.Empty));

        _resiliencePolicy.ExecuteAsync(Arg.Any<Func<Task<(bool, string, string)>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<Task<(bool, string, string)>>>()());

        _mapper.Map<EmailLogDto>(emailLog).Returns(new EmailLogDto());

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));


        // Act
        var result = await _sut.ExecuteAsync(sendBookingEmailDto);

        // Assert
        Assert.True(result.IsSuccess);
        await _emailLogRepository.Received(1).InsertAsync(Arg.Any<EmailLog>(), Arg.Any<CancellationToken>());
        await _emailService.Received(1).SendEmailAsync(
            sendBookingEmailDto.Email,
            "booking@sender.com",
            "Sender",
            "New Booking Confirmation",
            "htmlContent",
            cancellationToken: Arg.Any<CancellationToken>());
        await _emailLogRepository.Received(1).UpdateAsync(Arg.Any<EmailLog>(), Arg.Any<CancellationToken>());
        Assert.Equal(EmailStatus.Sent, emailLog.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenEmailFailsToSend()
    {
        // Arrange
        var sendBookingEmailDto = new SendBookingEmailDto
        {
            Email = "test@example.com",
            PlayerName = "Test Player",
            CourtName = "Test Court",
            BookingDate = "2026-01-15",
            TimeStr = "10:00 AM",
            PaymentLink = "http://example.com/payment"
        };

        var emailLog = new EmailLog(EmailType.Booking, EmailStatus.Queued, EmailPriority.High, "test@example.com", "booking@sender.com", "New Booking Confirmation", "htmlContent");

        _templateProvider.GetBookingTemplate(
            sendBookingEmailDto.PlayerName,
            sendBookingEmailDto.CourtName,
            sendBookingEmailDto.BookingDate,
            sendBookingEmailDto.TimeStr,
            sendBookingEmailDto.PaymentLink).Returns("htmlContent");

        _emailManager.GetPriorityForEmailType(EmailType.Booking).Returns(EmailPriority.High);
        _emailManager.CreateEmailLog(
            EmailType.Booking,
            EmailPriority.High,
            sendBookingEmailDto.Email,
            "booking@sender.com",
            "New Booking Confirmation",
            "htmlContent").Returns(emailLog);

        _emailService.SendEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns((false, null, "error"));

        _resiliencePolicy.ExecuteAsync(Arg.Any<Func<Task<(bool, string, string)>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<Task<(bool, string, string)>>>()());

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        // Act
        var result = await _sut.ExecuteAsync(sendBookingEmailDto);

        // Assert
        Assert.False(result.IsSuccess);
        await _emailLogRepository.Received(1).InsertAsync(Arg.Any<EmailLog>(), Arg.Any<CancellationToken>());
        await _emailLogRepository.Received(1).UpdateAsync(Arg.Any<EmailLog>(), Arg.Any<CancellationToken>());
        Assert.Equal(EmailStatus.Failed, emailLog.Status);
    }
}