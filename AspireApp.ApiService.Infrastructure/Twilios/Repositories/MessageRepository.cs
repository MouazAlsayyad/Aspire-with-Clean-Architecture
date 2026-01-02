using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Twilio.Domain.Entities;
using AspireApp.Twilio.Domain.Enums;
using AspireApp.Twilio.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Twilios.Repositories;

public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Message?> GetByMessageSidAsync(string messageSid, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Message>()
            .FirstOrDefaultAsync(m => m.MessageSid == messageSid && !m.IsDeleted, cancellationToken);
    }

    public async Task<List<Message>> GetFailedMessagesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Message>()
            .Where(m => m.Status == MessageStatus.Failed && !m.IsDeleted)
            .OrderByDescending(m => m.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> GetMessagesByRecipientAsync(
        string phoneNumber,
        MessageChannel? channel = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phoneNumber.Replace(" ", "").Trim();
        var query = _context.Set<Message>()
            .Where(m => m.RecipientPhoneNumber == normalizedPhone && !m.IsDeleted);

        if (channel.HasValue)
        {
            query = query.Where(m => m.Channel == channel.Value);
        }

        return await query
            .OrderByDescending(m => m.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> GetMessagesByStatusAsync(
        MessageStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Message>()
            .Where(m => m.Status == status && !m.IsDeleted)
            .OrderByDescending(m => m.CreationTime)
            .ToListAsync(cancellationToken);
    }
}

