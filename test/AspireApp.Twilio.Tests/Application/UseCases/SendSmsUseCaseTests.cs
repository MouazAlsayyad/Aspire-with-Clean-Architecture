using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Application.UseCases;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;
using AspireApp.Twilio.Domain.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.Twilio.Tests.Application.UseCases;

public class SendSmsUseCaseTests
{
    private readonly ITwilioSmsManager _twilioSmsManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly SendSmsUseCase _sut;

    public SendSmsUseCaseTests()
    {
        _twilioSmsManager = Substitute.For<ITwilioSmsManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        _sut = new SendSmsUseCase(
            _twilioSmsManager,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenSmsIsSentSuccessfully()
    {
        // Arrange
        var sendSmsDto = new SendSmsDto(
            ToPhoneNumber: "+1234567890",
            Message: "Test SMS"
        );

        var messageEntity = new Message(
            recipientPhoneNumber: sendSmsDto.ToPhoneNumber,
            messageBody: sendSmsDto.Message,
            channel: MessageChannel.SMS
        );
        messageEntity.MarkAsSent("SM1234567890");

        _twilioSmsManager.SendSmsAsync(
            sendSmsDto.ToPhoneNumber,
            sendSmsDto.Message,
            Arg.Any<CancellationToken>())
            .Returns(messageEntity);

        _mapper.Map<MessageDto>(messageEntity).Returns(new MessageDto(
            Id: messageEntity.Id,
            RecipientPhoneNumber: messageEntity.RecipientPhoneNumber,
            MessageBody: messageEntity.MessageBody,
            Channel: messageEntity.Channel,
            Status: messageEntity.Status, // Corrected: Removed incorrect cast
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
        var result = await _sut.ExecuteAsync(sendSmsDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(messageEntity.Id, result.Value.Id);
        Assert.Equal(messageEntity.RecipientPhoneNumber, result.Value.RecipientPhoneNumber);
        Assert.Equal(messageEntity.MessageBody, result.Value.MessageBody);
        await _twilioSmsManager.Received(1).SendSmsAsync(
            sendSmsDto.ToPhoneNumber,
            sendSmsDto.Message,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenSmsSendingFails()
    {
        // Arrange
        var sendSmsDto = new SendSmsDto(
            ToPhoneNumber: "+1234567890",
            Message: "Test SMS"
        );

        _twilioSmsManager.SendSmsAsync(
            sendSmsDto.ToPhoneNumber,
            sendSmsDto.Message,
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Message>(new Exception("Twilio error"))); // Simulate failure

        // Act
        var result = await _sut.ExecuteAsync(sendSmsDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Twilio error", result.Error.Message);
        await _twilioSmsManager.Received(1).SendSmsAsync(
            sendSmsDto.ToPhoneNumber,
            sendSmsDto.Message,
            Arg.Any<CancellationToken>());
    }
}