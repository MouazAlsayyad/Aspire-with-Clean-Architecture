using AspireApp.ApiService.Domain.ActivityLogs.Entities;
using AspireApp.ApiService.Domain.Enums;

namespace AspireApp.ApiService.Domain.ActivityLogs.Interfaces;

/// <summary>
/// Interface for storing and retrieving activity logs
/// </summary>
public interface IActivityLogStore
{
    /// <summary>
    /// Saves an activity log entry
    /// </summary>
    Task<ActivityLog> SaveAsync(ActivityLog activityLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of activity logs with optional filtering
    /// </summary>
    Task<(List<ActivityLog> Items, int TotalCount)> GetListAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? activityType = null,
        Guid? userId = null,
        Guid? entityId = null,
        string? entityType = null,
        ActivitySeverity? severity = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool? isPublic = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific activity log by ID
    /// </summary>
    Task<ActivityLog?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets activity logs for a specific user
    /// </summary>
    Task<List<ActivityLog>> GetUserActivitiesAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets activity logs for a specific entity
    /// </summary>
    Task<List<ActivityLog>> GetEntityActivitiesAsync(
        Guid entityId,
        string? entityType = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets activity statistics
    /// </summary>
    Task<Dictionary<string, object>> GetStatisticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes old logs (cleanup operation)
    /// </summary>
    Task<int> DeleteOldLogsAsync(DateTime olderThan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets activity logs by activity type
    /// </summary>
    Task<List<ActivityLog>> GetByActivityTypeAsync(
        string activityType,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}

