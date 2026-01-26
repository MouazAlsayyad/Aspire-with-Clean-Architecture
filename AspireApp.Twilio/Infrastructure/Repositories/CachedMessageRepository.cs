using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Interfaces;
using AspireApp.Twilio.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AspireApp.Twilio.Infrastructure.Repositories;

public class CachedMessageRepository : CachedRepository<Message>, IMessageRepository
{
    private readonly IMessageRepository _messageRepository;

    public CachedMessageRepository(
        IMessageRepository decorated,
        ICacheService cacheService,
        ILogger<CachedMessageRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _messageRepository = decorated;
    }

    public async Task<Message?> GetByMessageSidAsync(string messageSid, CancellationToken cancellationToken = default)
    {
        string key = $"message:sid:{messageSid}";
        
        return await _cacheService.GetOrSetAsync(
            key,
            ct => _messageRepository.GetByMessageSidAsync(messageSid, ct),
            TimeSpanConstants.OneHour,
            cancellationToken);
    }

    public async Task<List<Message>> GetFailedMessagesAsync(CancellationToken cancellationToken = default)
    {
        return await _messageRepository.GetFailedMessagesAsync(cancellationToken);
    }

    public async Task<List<Message>> GetMessagesByRecipientAsync(string phoneNumber, MessageChannel? channel = null, CancellationToken cancellationToken = default)
    {
        string key = $"message:recipient:{phoneNumber}:{channel}";
        
        var result = await _cacheService.GetOrSetAsync(
            key,
            ct => _messageRepository.GetMessagesByRecipientAsync(phoneNumber, channel, ct),
            TimeSpanConstants.TenMinutes,
            cancellationToken);

        return result ?? new List<Message>();
    }

    public async Task<List<Message>> GetMessagesByStatusAsync(MessageStatus status, CancellationToken cancellationToken = default)
    {
        // Avoid caching status lists as they change frequently
        return await _messageRepository.GetMessagesByStatusAsync(status, cancellationToken);
    }
}
