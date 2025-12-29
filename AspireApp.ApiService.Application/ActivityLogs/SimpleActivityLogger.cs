using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Interfaces;
using System.Text.Json;

namespace AspireApp.ApiService.Application.ActivityLogs;

/// <summary>
/// Simple implementation of IActivityLogger without HTTP context integration
/// </summary>
public class SimpleActivityLogger : IActivityLogger
{
    private readonly IActivityLogStore _activityLogStore;

    public SimpleActivityLogger(IActivityLogStore activityLogStore)
    {
        _activityLogStore = activityLogStore;
    }

    public async Task LogAsync(
        string activityType,
        string descriptionTemplateKey,
        Dictionary<string, object>? descriptionParameters = null,
        Guid? entityId = null,
        string? entityType = null,
        Dictionary<string, object>? metadata = null,
        ActivitySeverity? severity = null,
        bool? isPublic = null,
        params string[] tags)
    {
        try
        {
            var descriptionParamsJson = descriptionParameters != null
                ? JsonSerializer.Serialize(descriptionParameters)
                : null;

            var metadataJson = metadata != null
                ? JsonSerializer.Serialize(metadata)
                : null;

            var tagsString = tags != null && tags.Length > 0
                ? string.Join(",", tags)
                : null;

            var activityLog = new ActivityLog(
                activityType: activityType,
                descriptionTemplate: descriptionTemplateKey,
                userId: null,
                userName: null,
                entityId: entityId,
                entityType: entityType,
                descriptionParameters: descriptionParamsJson,
                metadata: metadataJson,
                ipAddress: null,
                userAgent: null,
                severity: severity ?? ActivitySeverity.Info,
                isPublic: isPublic ?? true,
                tags: tagsString);

            await _activityLogStore.SaveAsync(activityLog);
        }
        catch (Exception)
        {
            // Silently fail - logging should not break the application
            // In production, you might want to log to application logger
        }
    }

    public async Task LogAsync(
        string activityType,
        string description,
        Guid? entityId = null,
        string? entityType = null,
        Dictionary<string, object>? metadata = null,
        ActivitySeverity? severity = null,
        bool? isPublic = null,
        params string[] tags)
    {
        await LogAsync(
            activityType: activityType,
            descriptionTemplateKey: description,
            descriptionParameters: null,
            entityId: entityId,
            entityType: entityType,
            metadata: metadata,
            severity: severity,
            isPublic: isPublic,
            tags: tags);
    }

    public async Task LogAsync<TEntity>(
        string activityType,
        string descriptionTemplateKey,
        Guid entityId,
        Dictionary<string, object>? descriptionParameters = null,
        Dictionary<string, object>? metadata = null,
        ActivitySeverity? severity = null,
        bool? isPublic = null,
        params string[] tags) where TEntity : class
    {
        await LogAsync(
            activityType: activityType,
            descriptionTemplateKey: descriptionTemplateKey,
            descriptionParameters: descriptionParameters,
            entityId: entityId,
            entityType: typeof(TEntity).Name,
            metadata: metadata,
            severity: severity,
            isPublic: isPublic,
            tags: tags);
    }
}

