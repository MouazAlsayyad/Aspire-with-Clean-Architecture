using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AspireApp.ApiService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ActivityLog entity
/// </summary>
public class ActivityLogRepository : IActivityLogStore
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ActivityLog> _dbSet;

    public ActivityLogRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ActivityLog>();
    }

    public async Task<ActivityLog> SaveAsync(ActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        // ActivityLog inherits from BaseEntity but we don't want soft delete filtering for logs
        // Logs are permanent records
        await _dbSet.AddAsync(activityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return activityLog;
    }

    public async Task<(List<ActivityLog> Items, int TotalCount)> GetListAsync(
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
        CancellationToken cancellationToken = default)
    {
        // Ignore soft delete filter for activity logs - they are permanent records
        var query = _dbSet.IgnoreQueryFilters().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(activityType))
        {
            query = query.Where(x => x.ActivityType == activityType);
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (entityId.HasValue)
        {
            query = query.Where(x => x.EntityId == entityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(x => x.EntityType == entityType);
        }

        if (severity.HasValue)
        {
            query = query.Where(x => x.Severity == severity.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.CreationTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.CreationTime <= endDate.Value);
        }

        if (isPublic.HasValue)
        {
            query = query.Where(x => x.IsPublic == isPublic.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLowerInvariant();
            query = query.Where(x =>
                x.DescriptionTemplate.ToLower().Contains(searchLower) ||
                (x.UserName != null && x.UserName.ToLower().Contains(searchLower)) ||
                (x.ActivityType.ToLower().Contains(searchLower)));
        }

        // Order by creation time descending (newest first)
        query = query.OrderByDescending(x => x.CreationTime);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 50;
        var skip = (pageNumber - 1) * pageSize;
        query = query.Skip(skip).Take(pageSize);

        var items = await query.ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ActivityLog?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Ignore soft delete filter for activity logs
        return await _dbSet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<ActivityLog>> GetUserActivitiesAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var (items, _) = await GetListAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            userId: userId,
            cancellationToken: cancellationToken);

        return items;
    }

    public async Task<List<ActivityLog>> GetEntityActivitiesAsync(
        Guid entityId,
        string? entityType = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var (items, _) = await GetListAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            entityId: entityId,
            entityType: entityType,
            cancellationToken: cancellationToken);

        return items;
    }

    public async Task<Dictionary<string, object>> GetStatisticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.IgnoreQueryFilters().AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(x => x.CreationTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.CreationTime <= endDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var bySeverity = await query
            .GroupBy(x => x.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity.ToString(), x => (object)x.Count, cancellationToken);

        var byActivityType = await query
            .GroupBy(x => x.ActivityType)
            .Select(g => new { ActivityType = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToDictionaryAsync(x => x.ActivityType, x => (object)x.Count, cancellationToken);

        return new Dictionary<string, object>
        {
            ["TotalCount"] = totalCount,
            ["BySeverity"] = bySeverity,
            ["TopActivityTypes"] = byActivityType
        };
    }

    public async Task<int> DeleteOldLogsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        // Hard delete old logs (permanent removal)
        var oldLogs = await _dbSet.IgnoreQueryFilters()
            .Where(x => x.CreationTime < olderThan)
            .ToListAsync(cancellationToken);

        if (oldLogs.Any())
        {
            _dbSet.RemoveRange(oldLogs);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return oldLogs.Count;
    }

    public async Task<List<ActivityLog>> GetByActivityTypeAsync(
        string activityType,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var (items, _) = await GetListAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            activityType: activityType,
            cancellationToken: cancellationToken);

        return items;
    }
}

