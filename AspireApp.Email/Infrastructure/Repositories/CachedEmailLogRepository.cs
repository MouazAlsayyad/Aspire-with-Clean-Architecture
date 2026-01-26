using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Domain.Entities;
using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AspireApp.Email.Infrastructure.Repositories;

public class CachedEmailLogRepository : CachedRepository<EmailLog>, IEmailLogRepository
{
    private readonly IEmailLogRepository _emailLogRepository;

    public CachedEmailLogRepository(
        IEmailLogRepository decorated,
        ICacheService cacheService,
        ILogger<CachedEmailLogRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _emailLogRepository = decorated;
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
        // Complex search queries are hard to cache effectively and invalidation is difficult.
        // We will pass through to the underlying repository.
        return await _emailLogRepository.GetEmailLogsAsync(
            emailType, status, fromDate, toDate, skip, take, cancellationToken);
    }

    public async Task<List<EmailLog>> GetFailedEmailsForRetryAsync(int maxRetryCount = 3, CancellationToken cancellationToken = default)
    {
        // Don't cache retry list
        return await _emailLogRepository.GetFailedEmailsForRetryAsync(maxRetryCount, cancellationToken);
    }
}
