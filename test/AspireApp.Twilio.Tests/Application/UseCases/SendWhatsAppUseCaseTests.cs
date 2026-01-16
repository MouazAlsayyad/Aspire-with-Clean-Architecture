using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Application.UseCases;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;
using AspireApp.Twilio.Domain.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.Twilio.Tests.Application.UseCases;

public class SendWhatsAppUseCaseTests
{
    private readonly ITwilioSmsManager _twilioSmsManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly SendWhatsAppUseCase _sut;

    public SendWhatsAppUseCaseTests()
    {
        _twilioSmsManager = Substitute.For<ITwilioSmsManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        _sut = new SendWhatsAppUseCase(
            _twilioSmsManager,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenWhatsAppIsSentSuccessfully()
    {
        // Arrange
        var sendWhatsAppDto = new SendWhatsAppDto(
            ToPhoneNumber: "+1234567890",
            Message: "Test WhatsApp"
        );

        var messageEntity = new Message(
            recipientPhoneNumber: sendWhatsAppDto.ToPhoneNumber,
            messageBody: sendWhatsAppDto.Message!,
            channel: MessageChannel.WhatsApp
        );
        messageEntity.MarkAsSent("WA1234567890");

        _twilioSmsManager.SendWhatsAppAsync(
            sendWhatsAppDto.ToPhoneNumber,
            sendWhatsAppDto.Message!,
            Arg.Any<CancellationToken>())
            .Returns(messageEntity);

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
        var result = await _sut.ExecuteAsync(sendWhatsAppDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(messageEntity.Id, result.Value.Id);
        Assert.Equal(messageEntity.RecipientPhoneNumber, result.Value.RecipientPhoneNumber);
        Assert.Equal(messageEntity.MessageBody, result.Value.MessageBody);
        await _twilioSmsManager.Received(1).SendWhatsAppAsync(
            sendWhatsAppDto.ToPhoneNumber,
            sendWhatsAppDto.Message!,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenWhatsAppWithTemplateIsSentSuccessfully()
    {
        // Arrange
        var templateVariables = new Dictionary<string, object> { { "name", "Test User" } };
        var sendWhatsAppDto = new SendWhatsAppDto(
            ToPhoneNumber: "+1234567890",
            TemplateId: "my_template",
            TemplateVariables: templateVariables
        );

        var messageEntity = new Message(
            recipientPhoneNumber: sendWhatsAppDto.ToPhoneNumber,
            messageBody: "", // Template messages might have empty body in entity
            channel: MessageChannel.WhatsApp,
            templateId: sendWhatsAppDto.TemplateId,
            templateVariables: System.Text.Json.JsonSerializer.Serialize(templateVariables)
        );
        messageEntity.MarkAsSent("WA1234567891");

        _twilioSmsManager.SendWhatsAppWithTemplateAsync(
            sendWhatsAppDto.ToPhoneNumber,
            sendWhatsAppDto.TemplateId!,
            sendWhatsAppDto.TemplateVariables!,
            null,
            Arg.Any<CancellationToken>())
            .Returns(messageEntity);

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
        var result = await _sut.ExecuteAsync(sendWhatsAppDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(messageEntity.Id, result.Value.Id);
        await _twilioSmsManager.Received(1).SendWhatsAppWithTemplateAsync(
            sendWhatsAppDto.ToPhoneNumber,
            sendWhatsAppDto.TemplateId!,
            sendWhatsAppDto.TemplateVariables!,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenWhatsAppSendingFails()
    {
        // Arrange
        var sendWhatsAppDto = new SendWhatsAppDto(
            ToPhoneNumber: "+1234567890",
            Message: "Test WhatsApp"
        );

        _twilioSmsManager.SendWhatsAppAsync(
            sendWhatsAppDto.ToPhoneNumber,
            sendWhatsAppDto.Message!,
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Message>(new Exception("WhatsApp error")));

        // Act
        var result = await _sut.ExecuteAsync(sendWhatsAppDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("WhatsApp error", result.Error.Message);
        await _twilioSmsManager.Received(1).SendWhatsAppAsync(
            sendWhatsAppDto.ToPhoneNumber,
            sendWhatsAppDto.Message!,
            Arg.Any<CancellationToken>());
    }
}