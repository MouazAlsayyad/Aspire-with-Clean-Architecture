using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;

namespace AspireApp.Twilio.Domain.Interfaces;

/// <summary>
/// Repository interface for Message entity
/// </summary>
public interface IMessageRepository : IRepository<Message>
{
    /// <summary>
    /// Gets a message by Twilio Message SID
    /// </summary>
    Task<Message?> GetByMessageSidAsync(string messageSid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed messages
    /// </summary>
    Task<List<Message>> GetFailedMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets messages by recipient phone number
    /// </summary>
    Task<List<Message>> GetMessagesByRecipientAsync(
        string phoneNumber,
        MessageChannel? channel = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets messages by status
    /// </summary>
    Task<List<Message>> GetMessagesByStatusAsync(
        MessageStatus status,
        CancellationToken cancellationToken = default);
}

