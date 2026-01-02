using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Email.Domain.Entities;
using AspireApp.Email.Domain.Enums;
using AspireApp.Email.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Email.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for EmailLog entity
/// </summary>
public class EmailLogRepository : Repository<EmailLog>, IEmailLogRepository
{
    public EmailLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(List<EmailLog> EmailLogs, int TotalCount)> GetEmailLogsAsync(
        EmailType? emailType = null,
        EmailStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        IQueryable<EmailLog> query = _context.Set<EmailLog>()
            .Where(e => !e.IsDeleted);

        // Apply filters
        if (emailType.HasValue)
        {
            query = query.Where(e => e.EmailType == emailType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.CreationTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.CreationTime <= toDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply ordering and pagination
        var emailLogs = await query
            .OrderByDescending(e => e.CreationTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (emailLogs, totalCount);
    }

    public async Task<List<EmailLog>> GetFailedEmailsForRetryAsync(
        int maxRetryCount = 3,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<EmailLog>()
            .Where(e => !e.IsDeleted &&
                       e.Status == EmailStatus.Failed &&
                       e.RetryCount < maxRetryCount)
            .OrderBy(e => e.CreationTime)
            .Take(100) // Limit to 100 emails per batch
            .ToListAsync(cancellationToken);
    }
}

