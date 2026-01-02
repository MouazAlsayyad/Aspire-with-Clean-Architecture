using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Domain.Entities;
using AspireApp.Email.Domain.Enums;

namespace AspireApp.Email.Domain.Interfaces;

/// <summary>
/// Repository interface for EmailLog entity
/// </summary>
public interface IEmailLogRepository : IRepository<EmailLog>
{
    /// <summary>
    /// Gets paginated email logs with optional filtering
    /// </summary>
    Task<(List<EmailLog> EmailLogs, int TotalCount)> GetEmailLogsAsync(
        EmailType? emailType = null,
        EmailStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed emails that need retry
    /// </summary>
    Task<List<EmailLog>> GetFailedEmailsForRetryAsync(
        int maxRetryCount = 3,
        CancellationToken cancellationToken = default);
}

