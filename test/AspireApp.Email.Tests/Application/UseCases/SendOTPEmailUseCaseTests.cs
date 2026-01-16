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

public class SendOTPEmailUseCaseTests
{
    private readonly IEmailManager _emailManager;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IResiliencePolicy _resiliencePolicy;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly SendOTPEmailUseCase _sut;

    public SendOTPEmailUseCaseTests()
    {
        _emailManager = Substitute.For<IEmailManager>();
        _emailService = Substitute.For<IEmailService>();
        _templateProvider = Substitute.For<IEmailTemplateProvider>();
        _emailLogRepository = Substitute.For<IEmailLogRepository>();
        _resiliencePolicy = Substitute.For<IResiliencePolicy>();
        _configuration = Substitute.For<IConfiguration>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        _configuration["Email:SenderEmail"].Returns("booking@example.com");
        _configuration["Email:SenderName"].Returns("Booking");

        _sut = new SendOTPEmailUseCase(
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
        var sendOTPEmailDto = new SendOTPEmailDto
        {
            Email = "test@example.com",
            ClubName = "Test Club",
            Otp = "123456"
        };

        var emailLog = new EmailLog(EmailType.OTP, EmailStatus.Queued, EmailPriority.High, "test@example.com", "booking@example.com", "OTP Confirmation", "htmlContent");

        _templateProvider.GetOtpTemplate(sendOTPEmailDto.ClubName, sendOTPEmailDto.Otp).Returns("htmlContent");

        _emailManager.GetPriorityForEmailType(EmailType.OTP).Returns(EmailPriority.High);
        _emailManager.CreateEmailLog(
            EmailType.OTP,
            EmailPriority.High,
            sendOTPEmailDto.Email,
            "booking@example.com",
            "OTP Confirmation",
            "htmlContent").Returns(emailLog);

        _emailService.SendEmailAsync(
            sendOTPEmailDto.Email,
            "booking@example.com",
            "Booking",
            "OTP Confirmation",
            "htmlContent",
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns((true, "messageId", string.Empty));

        _resiliencePolicy.ExecuteAsync(Arg.Any<Func<Task<(bool, string, string)>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<Task<(bool, string, string)>>>()());

        _mapper.Map<EmailLogDto>(emailLog).Returns(new EmailLogDto());

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        // Act
        var result = await _sut.ExecuteAsync(sendOTPEmailDto);

        // Assert
        Assert.True(result.IsSuccess);
        await _emailLogRepository.Received(1).InsertAsync(Arg.Any<EmailLog>(), Arg.Any<CancellationToken>());
        await _emailService.Received(1).SendEmailAsync(
            sendOTPEmailDto.Email,
            "booking@example.com",
            "Booking",
            "OTP Confirmation",
            "htmlContent",
            cancellationToken: Arg.Any<CancellationToken>());
        await _emailLogRepository.Received(1).UpdateAsync(Arg.Any<EmailLog>(), Arg.Any<CancellationToken>());
        Assert.Equal(EmailStatus.Sent, emailLog.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenEmailFailsToSend()
    {
        // Arrange
        var sendOTPEmailDto = new SendOTPEmailDto
        {
            Email = "test@example.com",
            ClubName = "Test Club",
            Otp = "123456"
        };

        var emailLog = new EmailLog(EmailType.OTP, EmailStatus.Queued, EmailPriority.High, "test@example.com", "booking@example.com", "OTP Confirmation", "htmlContent");

        _templateProvider.GetOtpTemplate(sendOTPEmailDto.ClubName, sendOTPEmailDto.Otp).Returns("htmlContent");

        _emailManager.GetPriorityForEmailType(EmailType.OTP).Returns(EmailPriority.High);
        _emailManager.CreateEmailLog(
            EmailType.OTP,
            EmailPriority.High,
            sendOTPEmailDto.Email,
            "booking@example.com",
            "OTP Confirmation",
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
        var result = await _sut.ExecuteAsync(sendOTPEmailDto);

        // Assert
        Assert.False(result.IsSuccess);
        await _emailLogRepository.Received(1).InsertAsync(Arg.Any<EmailLog>(), Arg.Any<CancellationToken>());
        await _emailLogRepository.Received(1).UpdateAsync(Arg.Any<EmailLog>(), Arg.Any<CancellationToken>());
        Assert.Equal(EmailStatus.Failed, emailLog.Status);
    }
}