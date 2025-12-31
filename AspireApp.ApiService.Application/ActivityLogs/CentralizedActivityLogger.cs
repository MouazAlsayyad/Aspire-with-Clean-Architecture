using AspireApp.ApiService.Domain.ActivityLogs.Entities;
using AspireApp.ApiService.Domain.ActivityLogs.Interfaces;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace AspireApp.ApiService.Application.ActivityLogs;

/// <summary>
/// Enhanced activity logger with HTTP context integration
/// Automatically extracts user information, IP address, and user agent from HTTP context
/// </summary>
public class CentralizedActivityLogger : IActivityLogger
{
    private readonly IActivityLogStore _activityLogStore;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CentralizedActivityLogger(
        IActivityLogStore activityLogStore,
        IHttpContextAccessor httpContextAccessor)
    {
        _activityLogStore = activityLogStore;
        _httpContextAccessor = httpContextAccessor;
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
            var (userId, userName) = GetUserInfo();
            
            // Skip logging for admin users if configured (optional)
            // You can add admin filtering logic here if needed

            var descriptionParamsJson = descriptionParameters != null
                ? JsonSerializer.Serialize(descriptionParameters)
                : null;

            var metadataJson = metadata != null
                ? JsonSerializer.Serialize(metadata)
                : null;

            var tagsString = tags != null && tags.Length > 0
                ? string.Join(",", tags)
                : null;

            var (ipAddress, userAgent) = GetHttpContextInfo();

            var activityLog = new ActivityLog(
                activityType: activityType,
                descriptionTemplate: descriptionTemplateKey,
                userId: userId,
                userName: userName,
                entityId: entityId,
                entityType: entityType,
                descriptionParameters: descriptionParamsJson,
                metadata: metadataJson,
                ipAddress: ipAddress,
                userAgent: userAgent,
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

    /// <summary>
    /// Extracts user information from HTTP context
    /// </summary>
    private (Guid? userId, string? userName) GetUserInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return (null, null);
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        var userNameClaim = httpContext.User.FindFirst(ClaimTypes.Name);

        Guid? userId = null;
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var userName = userNameClaim?.Value;

        return (userId, userName);
    }

    /// <summary>
    /// Extracts IP address and user agent from HTTP context
    /// </summary>
    private (string? ipAddress, string? userAgent) GetHttpContextInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return (null, null);
        }

        // Get IP address (considering proxies)
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        
        // Check for forwarded IP (if behind proxy/load balancer)
        if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                // Take the first IP if multiple are present
                ipAddress = forwardedFor.Split(',')[0].Trim();
            }
        }
        else if (httpContext.Request.Headers.ContainsKey("X-Real-IP"))
        {
            ipAddress = httpContext.Request.Headers["X-Real-IP"].ToString();
        }

        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

        return (ipAddress, userAgent);
    }
}

