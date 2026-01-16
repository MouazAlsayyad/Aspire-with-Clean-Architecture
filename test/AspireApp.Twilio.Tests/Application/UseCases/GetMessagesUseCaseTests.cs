using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Application.UseCases;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;
using AspireApp.Twilio.Domain.Interfaces;
using AspireApp.Twilio.Tests.Helpers;
using AutoMapper;
using NSubstitute;

namespace AspireApp.Twilio.Tests.Application.UseCases;

public class GetMessagesUseCaseTests
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly GetMessagesUseCase _sut;

    public GetMessagesUseCaseTests()
    {
        _messageRepository = Substitute.For<IMessageRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        _sut = new GetMessagesUseCase(
            _messageRepository,
            _unitOfWork,
            _mapper);
    }

    private List<Message> GetSampleMessagesList()
    {
        var message1 = new Message("111", "SMS 1", MessageChannel.SMS);
        message1.UpdateStatus(MessageStatus.Sent);
        message1.SetCreationTime(DateTime.UtcNow.AddMinutes(-5));

        var message2 = new Message("222", "SMS 2", MessageChannel.SMS);
        message2.UpdateStatus(MessageStatus.Delivered);
        message2.SetCreationTime(DateTime.UtcNow.AddMinutes(-10));

        var message3 = new Message("111", "WhatsApp 1", MessageChannel.WhatsApp);
        message3.UpdateStatus(MessageStatus.Sent);
        message3.SetCreationTime(DateTime.UtcNow.AddMinutes(-1));

        var message4 = new Message("333", "SMS 3", MessageChannel.SMS);
        message4.UpdateStatus(MessageStatus.Failed);
        message4.SetCreationTime(DateTime.UtcNow.AddMinutes(-15));

        return new List<Message> { message1, message2, message3, message4 };
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllMessages_WhenNoFiltersApplied()
    {
        // Arrange
        var messagesList = GetSampleMessagesList();
        _messageRepository.GetQueryableAsync().Returns(messagesList.AsAsyncQueryable());

        _mapper.Map<List<MessageDto>>(Arg.Any<List<Message>>()).Returns(x =>
            x.Arg<List<Message>>().Select(m => new MessageDto(
                Id: m.Id,
                RecipientPhoneNumber: m.RecipientPhoneNumber,
                MessageBody: m.MessageBody,
                Channel: m.Channel,
                Status: m.Status,
                MessageSid: m.MessageSid,
                SentAt: m.SentAt,
                DeliveredAt: m.DeliveredAt,
                FailedAt: m.FailedAt,
                FailureReason: m.FailureReason,
                TemplateId: m.TemplateId,
                TemplateVariables: m.TemplateVariables,
                CreationTime: m.CreationTime
            )).ToList());

        var request = new GetMessagesRequestDto();

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(4, result.Value.Count);
        Assert.Equal(4, result.Pagination?.TotalCount);
        await _messageRepository.Received(1).GetQueryableAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFilterByPhoneNumber()
    {
        // Arrange
        var messagesList = GetSampleMessagesList();
        _messageRepository.GetQueryableAsync().Returns(messagesList.AsAsyncQueryable());

        _mapper.Map<List<MessageDto>>(Arg.Any<List<Message>>()).Returns(x =>
            x.Arg<List<Message>>().Select(m => new MessageDto(
                Id: m.Id,
                RecipientPhoneNumber: m.RecipientPhoneNumber,
                MessageBody: m.MessageBody,
                Channel: m.Channel,
                Status: m.Status,
                MessageSid: m.MessageSid,
                SentAt: m.SentAt,
                DeliveredAt: m.DeliveredAt,
                FailedAt: m.FailedAt,
                FailureReason: m.FailureReason,
                TemplateId: m.TemplateId,
                TemplateVariables: m.TemplateVariables,
                CreationTime: m.CreationTime
            )).ToList());

        var request = new GetMessagesRequestDto(PhoneNumber: "111");

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, m => Assert.Equal("111", m.RecipientPhoneNumber));
        await _messageRepository.Received(1).GetQueryableAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFilterByChannel()
    {
        // Arrange
        var messagesList = GetSampleMessagesList();
        _messageRepository.GetQueryableAsync().Returns(messagesList.AsAsyncQueryable());

        _mapper.Map<List<MessageDto>>(Arg.Any<List<Message>>()).Returns(x =>
            x.Arg<List<Message>>().Select(m => new MessageDto(
                Id: m.Id,
                RecipientPhoneNumber: m.RecipientPhoneNumber,
                MessageBody: m.MessageBody,
                Channel: m.Channel,
                Status: m.Status,
                MessageSid: m.MessageSid,
                SentAt: m.SentAt,
                DeliveredAt: m.DeliveredAt,
                FailedAt: m.FailedAt,
                FailureReason: m.FailureReason,
                TemplateId: m.TemplateId,
                TemplateVariables: m.TemplateVariables,
                CreationTime: m.CreationTime
            )).ToList());

        var request = new GetMessagesRequestDto(Channel: MessageChannel.SMS);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count);
        Assert.All(result.Value, m => Assert.Equal(MessageChannel.SMS, m.Channel));
        await _messageRepository.Received(1).GetQueryableAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFilterByStatus()
    {
        // Arrange
        var messagesList = GetSampleMessagesList();
        _messageRepository.GetQueryableAsync().Returns(messagesList.AsAsyncQueryable());

        _mapper.Map<List<MessageDto>>(Arg.Any<List<Message>>()).Returns(x =>
            x.Arg<List<Message>>().Select(m => new MessageDto(
                Id: m.Id,
                RecipientPhoneNumber: m.RecipientPhoneNumber,
                MessageBody: m.MessageBody,
                Channel: m.Channel,
                Status: m.Status,
                MessageSid: m.MessageSid,
                SentAt: m.SentAt,
                DeliveredAt: m.DeliveredAt,
                FailedAt: m.FailedAt,
                FailureReason: m.FailureReason,
                TemplateId: m.TemplateId,
                TemplateVariables: m.TemplateVariables,
                CreationTime: m.CreationTime
            )).ToList());

        var request = new GetMessagesRequestDto(Status: MessageStatus.Sent);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, m => Assert.Equal(MessageStatus.Sent, m.Status));
        await _messageRepository.Received(1).GetQueryableAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyPagination()
    {
        // Arrange
        var messagesList = GetSampleMessagesList(); // 4 messages
        _messageRepository.GetQueryableAsync().Returns(messagesList.AsAsyncQueryable());

        _mapper.Map<List<MessageDto>>(Arg.Any<List<Message>>()).Returns(x =>
            x.Arg<List<Message>>().Select(m => new MessageDto(
                Id: m.Id,
                RecipientPhoneNumber: m.RecipientPhoneNumber,
                MessageBody: m.MessageBody,
                Channel: m.Channel,
                Status: m.Status,
                MessageSid: m.MessageSid,
                SentAt: m.SentAt,
                DeliveredAt: m.DeliveredAt,
                FailedAt: m.FailedAt,
                FailureReason: m.FailureReason,
                TemplateId: m.TemplateId,
                TemplateVariables: m.TemplateVariables,
                CreationTime: m.CreationTime
            )).ToList());

        var request = new GetMessagesRequestDto(PageNumber: 2, PageSize: 2);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count); // Should get the last 2 messages
        Assert.Equal(4, result.Pagination?.TotalCount);
        Assert.Equal(2, result.Pagination?.PageNumber);
        Assert.Equal(2, result.Pagination?.PageSize);
        await _messageRepository.Received(1).GetQueryableAsync();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        _messageRepository.GetQueryableAsync().Returns(Task.FromException<IQueryable<Message>>(new Exception("Database error")));

        var request = new GetMessagesRequestDto();

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Database error", result.Error.Message);
        await _messageRepository.Received(1).GetQueryableAsync();
    }
}