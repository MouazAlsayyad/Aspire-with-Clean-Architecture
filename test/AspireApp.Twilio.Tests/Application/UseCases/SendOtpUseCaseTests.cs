using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Application.UseCases;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;
using AspireApp.Twilio.Domain.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.Twilio.Tests.Application.UseCases;

public class SendOtpUseCaseTests
{
    private readonly ITwilioSmsManager _twilioSmsManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly SendOtpUseCase _sut;

    public SendOtpUseCaseTests()
    {
        _twilioSmsManager = Substitute.For<ITwilioSmsManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        _sut = new SendOtpUseCase(
            _twilioSmsManager,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenOtpIsSentSuccessfully()
    {
        // Arrange
        var sendOtpDto = new SendOtpDto
        (
            PhoneNumber: "+1234567890",
            Name: "Test User"
        );

        var otpEntity = new Otp(
            phoneNumber: sendOtpDto.PhoneNumber,
            code: "123456",
            expirationMinutes: 5
        );

        var messageEntity = new Message(
            recipientPhoneNumber: sendOtpDto.PhoneNumber,
            messageBody: "Your OTP is 123456",
            channel: MessageChannel.SMS
        );
        messageEntity.MarkAsSent("SM1234567891");

        _twilioSmsManager.SendOtpAsync(
            sendOtpDto.PhoneNumber,
            sendOtpDto.Name,
            Arg.Any<CancellationToken>())
            .Returns((otpEntity, messageEntity));

        _mapper.Map<MessageDto>(messageEntity).Returns(new MessageDto(
            Id: messageEntity.Id,
            RecipientPhoneNumber: messageEntity.RecipientPhoneNumber,
            MessageBody: messageEntity.MessageBody,
            Channel: messageEntity.Channel,
            Status: messageEntity.Status,
            MessageSid: messageEntity.MessageSid,
            SentAt: messageEntity.SentAt,
            DeliveredAt: messageEntity.DeliveredAt,
            FailedAt: messageEntity.FailedAt,
            FailureReason: messageEntity.FailureReason,
            TemplateId: messageEntity.TemplateId,
            TemplateVariables: messageEntity.TemplateVariables,
            CreationTime: messageEntity.CreationTime
        ));

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));


        // Act
        var result = await _sut.ExecuteAsync(sendOtpDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(messageEntity.Id, result.Value.Id);
        Assert.Equal(messageEntity.RecipientPhoneNumber, result.Value.RecipientPhoneNumber);
        await _twilioSmsManager.Received(1).SendOtpAsync(
            sendOtpDto.PhoneNumber,
            sendOtpDto.Name,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenOtpSendingFails()
    {
        // Arrange
        var sendOtpDto = new SendOtpDto(
            PhoneNumber: "+1234567890",
            Name: "Test User"
        );

        _twilioSmsManager.SendOtpAsync(
            sendOtpDto.PhoneNumber,
            sendOtpDto.Name,
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<(Otp, Message)>(new Exception("OTP send error")));

        // Act
        var result = await _sut.ExecuteAsync(sendOtpDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("OTP send error", result.Error.Message);
        await _twilioSmsManager.Received(1).SendOtpAsync(
            sendOtpDto.PhoneNumber,
            sendOtpDto.Name,
            Arg.Any<CancellationToken>());
    }
}