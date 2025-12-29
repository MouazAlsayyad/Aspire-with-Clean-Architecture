# ActivityLog - Comprehensive Activity Logging System

A robust, centralized activity logging system for tracking user actions, system events, and entity changes throughout the AspireApp application.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Components](#components)
- [Usage Guide](#usage-guide)
- [Configuration](#configuration)
- [API Reference](#api-reference)
- [Best Practices](#best-practices)
- [Examples](#examples)

## 🎯 Overview

The ActivityLog system provides comprehensive activity tracking capabilities, allowing you to:
- Track user actions and system events
- Monitor entity changes and modifications
- Capture contextual information (IP address, user agent, etc.)
- Filter and query activity logs by various criteria
- Generate statistics and analytics
- Support both HTTP context-aware and standalone logging scenarios

Activity logs are **permanent records** - they do not support soft deletion and are designed to maintain a complete audit trail of system activities.

## 🏗️ Architecture

The ActivityLog system follows Clean Architecture principles and is organized across multiple layers:

```
┌─────────────────────────────────────────────────────────┐
│              Domain Layer                               │
│  ┌──────────────────────────────────────────────────┐  │
│  │ ActivityLog Entity                               │  │
│  │ IActivityLogger Interface                        │  │
│  │ IActivityLogStore Interface                      │  │
│  │ ActivitySeverity Enum                            │  │
│  └──────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│          Application Layer                              │
│  ┌──────────────────────────────────────────────────┐  │
│  │ CentralizedActivityLogger                        │  │
│  │ SimpleActivityLogger                             │  │
│  └──────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│        Infrastructure Layer                             │
│  ┌──────────────────────────────────────────────────┐  │
│  │ ActivityLogRepository                            │  │
│  │ Database Context Integration                     │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

#### Domain Layer (`AspireApp.ApiService.Domain`)
- **`ActivityLog` Entity**: Core domain model representing a logged activity
- **`IActivityLogger` Interface**: Contract for logging activities
- **`IActivityLogStore` Interface**: Contract for storing and retrieving activity logs
- **`ActivitySeverity` Enum**: Defines severity levels (Info, Low, Medium, High, Critical)

#### Application Layer (`AspireApp.ApiService.Application`)
- **`CentralizedActivityLogger`**: HTTP context-aware logger that automatically extracts user info, IP address, and user agent
- **`SimpleActivityLogger`**: Standalone logger without HTTP context dependencies

#### Infrastructure Layer (`AspireApp.ApiService.Infrastructure`)
- **`ActivityLogRepository`**: EF Core implementation of `IActivityLogStore`
- Database persistence and query operations

## ✨ Features

### Core Features

- ✅ **Template-Based Descriptions**: Support for parameterized description templates
- ✅ **Automatic Context Extraction**: HTTP context integration (user, IP, user agent)
- ✅ **Entity Tracking**: Link activities to specific entities (User, Order, etc.)
- ✅ **Severity Levels**: Categorize activities by importance (Info, Low, Medium, High, Critical)
- ✅ **Public/Private Logs**: Control visibility of logs to end users
- ✅ **Tagging System**: Categorize logs with custom tags
- ✅ **Rich Metadata**: Store additional JSON metadata with each log
- ✅ **Comprehensive Filtering**: Filter by user, entity, type, severity, date range, etc.
- ✅ **Pagination Support**: Efficient pagination for large log datasets
- ✅ **Statistics & Analytics**: Generate activity statistics and reports
- ✅ **Permanent Records**: Activity logs are never soft-deleted (audit trail integrity)

### Advanced Features

- **Dual Logger Implementations**: Choose between HTTP-aware or standalone logging
- **Silent Failure**: Logging failures don't break application flow
- **Proxy Support**: Automatic IP address extraction from X-Forwarded-For headers
- **Search Capabilities**: Full-text search across descriptions, usernames, and activity types

## 🔧 Components

### ActivityLog Entity

The core entity representing a logged activity:

```csharp
public class ActivityLog : BaseEntity
{
    public string ActivityType { get; private set; }           // e.g., "UserCreated", "OrderUpdated"
    public string DescriptionTemplate { get; private set; }   // Human-readable description
    public string? DescriptionParameters { get; private set; } // JSON parameters for template
    public Guid? UserId { get; private set; }                 // User who performed the activity
    public string? UserName { get; private set; }             // Username for quick reference
    public Guid? EntityId { get; private set; }               // Affected entity ID
    public string? EntityType { get; private set; }          // Affected entity type
    public string? Metadata { get; private set; }             // Additional JSON metadata
    public string? IpAddress { get; private set; }            // Client IP address
    public string? UserAgent { get; private set; }           // Browser/client user agent
    public ActivitySeverity Severity { get; private set; }    // Severity level
    public bool IsPublic { get; private set; }               // Visibility flag
    public string? Tags { get; private set; }                // Comma-separated tags
}
```

### ActivitySeverity Enum

Defines the severity levels for activities:

```csharp
public enum ActivitySeverity
{
    Info = 0,      // Informational activity (default)
    Low = 1,       // Low importance activity
    Medium = 2,    // Medium importance activity
    High = 3,      // High importance activity
    Critical = 4   // Critical activity requiring attention
}
```

### IActivityLogger Interface

Provides three overloads for logging activities:

1. **Template-based logging** with parameters
2. **Simple string description** logging
3. **Strongly-typed entity** logging

### Logger Implementations

#### CentralizedActivityLogger

Automatically extracts:
- User ID and username from HTTP context claims
- IP address (with proxy support via X-Forwarded-For headers)
- User agent from request headers

**Best for**: Web API endpoints, HTTP request handling

#### SimpleActivityLogger

Standalone logger without HTTP context dependencies.

**Best for**: Background jobs, console applications, non-HTTP scenarios

## 📖 Usage Guide

### Dependency Injection Setup

Register the logger and repository in your `Program.cs` or service configuration:

```csharp
// Register the store/repository
services.AddScoped<IActivityLogStore, ActivityLogRepository>();

// Register the logger (choose one based on your needs)
services.AddScoped<IActivityLogger, CentralizedActivityLogger>();
// OR
services.AddScoped<IActivityLogger, SimpleActivityLogger>();

// Required for CentralizedActivityLogger
services.AddHttpContextAccessor();
```

### Basic Usage

#### 1. Simple Activity Logging

```csharp
public class UserService
{
    private readonly IActivityLogger _activityLogger;

    public UserService(IActivityLogger activityLogger)
    {
        _activityLogger = activityLogger;
    }

    public async Task CreateUserAsync(CreateUserDto dto)
    {
        // ... create user logic ...

        // Log the activity
        await _activityLogger.LogAsync(
            activityType: "UserCreated",
            description: $"User {dto.Email} was created successfully",
            entityId: user.Id,
            entityType: "User",
            severity: ActivitySeverity.Info,
            tags: "user-management", "creation"
        );
    }
}
```

#### 2. Template-Based Logging with Parameters

```csharp
await _activityLogger.LogAsync(
    activityType: "OrderPlaced",
    descriptionTemplateKey: "Order {OrderNumber} was placed by {CustomerName}",
    descriptionParameters: new Dictionary<string, object>
    {
        ["OrderNumber"] = order.Number,
        ["CustomerName"] = customer.Name
    },
    entityId: order.Id,
    entityType: "Order",
    severity: ActivitySeverity.Medium,
    tags: "order", "purchase"
);
```

#### 3. Strongly-Typed Entity Logging

```csharp
await _activityLogger.LogAsync<Order>(
    activityType: "OrderUpdated",
    descriptionTemplateKey: "Order {OrderId} was updated",
    entityId: order.Id,
    descriptionParameters: new Dictionary<string, object>
    {
        ["OrderId"] = order.Id
    },
    severity: ActivitySeverity.Low
);
```

#### 4. Logging with Metadata

```csharp
await _activityLogger.LogAsync(
    activityType: "PaymentProcessed",
    description: "Payment was processed successfully",
    entityId: payment.Id,
    entityType: "Payment",
    metadata: new Dictionary<string, object>
    {
        ["Amount"] = payment.Amount,
        ["Currency"] = payment.Currency,
        ["PaymentMethod"] = payment.Method,
        ["TransactionId"] = payment.TransactionId
    },
    severity: ActivitySeverity.High,
    isPublic: false, // Internal-only log
    tags: "payment", "financial"
);
```

### Retrieving Activity Logs

#### Get Paginated List with Filtering

```csharp
public class ActivityLogService
{
    private readonly IActivityLogStore _activityLogStore;

    public ActivityLogService(IActivityLogStore activityLogStore)
    {
        _activityLogStore = activityLogStore;
    }

    public async Task<(List<ActivityLog> Items, int TotalCount)> GetActivitiesAsync(
        int pageNumber = 1,
        int pageSize = 50,
        Guid? userId = null,
        string? activityType = null,
        ActivitySeverity? severity = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return await _activityLogStore.GetListAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            userId: userId,
            activityType: activityType,
            severity: severity,
            startDate: startDate,
            endDate: endDate
        );
    }
}
```

#### Get User Activities

```csharp
var userActivities = await _activityLogStore.GetUserActivitiesAsync(
    userId: userId,
    pageNumber: 1,
    pageSize: 50
);
```

#### Get Entity Activities

```csharp
var orderActivities = await _activityLogStore.GetEntityActivitiesAsync(
    entityId: orderId,
    entityType: "Order",
    pageNumber: 1,
    pageSize: 50
);
```

#### Get Statistics

```csharp
var statistics = await _activityLogStore.GetStatisticsAsync(
    startDate: DateTime.UtcNow.AddDays(-30),
    endDate: DateTime.UtcNow
);

// Returns dictionary with:
// - TotalCount: Total number of logs
// - BySeverity: Count grouped by severity level
// - TopActivityTypes: Top 10 activity types by count
```

## ⚙️ Configuration

### Database Migration

Activity logs are stored in the database. Ensure the migration has been applied:

```bash
dotnet ef database update --project ../AspireApp.ApiService.Infrastructure
```

### Logger Selection

Choose the appropriate logger based on your use case:

**Use `CentralizedActivityLogger` when:**
- Logging from HTTP endpoints
- You need automatic user/IP/user agent extraction
- Working within ASP.NET Core request context

**Use `SimpleActivityLogger` when:**
- Logging from background jobs
- Logging from console applications
- Working outside HTTP context
- You want to manually specify user information

## 📚 API Reference

### IActivityLogger Methods

#### LogAsync (Template-based)

```csharp
Task LogAsync(
    string activityType,
    string descriptionTemplateKey,
    Dictionary<string, object>? descriptionParameters = null,
    Guid? entityId = null,
    string? entityType = null,
    Dictionary<string, object>? metadata = null,
    ActivitySeverity? severity = null,
    bool? isPublic = null,
    params string[] tags);
```

#### LogAsync (Simple)

```csharp
Task LogAsync(
    string activityType,
    string description,
    Guid? entityId = null,
    string? entityType = null,
    Dictionary<string, object>? metadata = null,
    ActivitySeverity? severity = null,
    bool? isPublic = null,
    params string[] tags);
```

#### LogAsync<TEntity> (Strongly-typed)

```csharp
Task LogAsync<TEntity>(
    string activityType,
    string descriptionTemplateKey,
    Guid entityId,
    Dictionary<string, object>? descriptionParameters = null,
    Dictionary<string, object>? metadata = null,
    ActivitySeverity? severity = null,
    bool? isPublic = null,
    params string[] tags) where TEntity : class;
```

### IActivityLogStore Methods

#### SaveAsync

```csharp
Task<ActivityLog> SaveAsync(
    ActivityLog activityLog,
    CancellationToken cancellationToken = default);
```

#### GetListAsync

```csharp
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
```

#### GetAsync

```csharp
Task<ActivityLog?> GetAsync(
    Guid id,
    CancellationToken cancellationToken = default);
```

#### GetUserActivitiesAsync

```csharp
Task<List<ActivityLog>> GetUserActivitiesAsync(
    Guid userId,
    int pageNumber = 1,
    int pageSize = 50,
    CancellationToken cancellationToken = default);
```

#### GetEntityActivitiesAsync

```csharp
Task<List<ActivityLog>> GetEntityActivitiesAsync(
    Guid entityId,
    string? entityType = null,
    int pageNumber = 1,
    int pageSize = 50,
    CancellationToken cancellationToken = default);
```

#### GetStatisticsAsync

```csharp
Task<Dictionary<string, object>> GetStatisticsAsync(
    DateTime? startDate = null,
    DateTime? endDate = null,
    CancellationToken cancellationToken = default);
```

#### DeleteOldLogsAsync

```csharp
Task<int> DeleteOldLogsAsync(
    DateTime olderThan,
    CancellationToken cancellationToken = default);
```

#### GetByActivityTypeAsync

```csharp
Task<List<ActivityLog>> GetByActivityTypeAsync(
    string activityType,
    int pageNumber = 1,
    int pageSize = 50,
    CancellationToken cancellationToken = default);
```

## 💡 Best Practices

### Activity Type Naming

Use consistent, descriptive activity type names:
- ✅ `UserCreated`, `UserUpdated`, `UserDeleted`
- ✅ `OrderPlaced`, `OrderCancelled`, `OrderShipped`
- ✅ `PaymentProcessed`, `PaymentFailed`
- ❌ `Create`, `Update`, `Delete` (too generic)
- ❌ `Action1`, `Action2` (not descriptive)

### Severity Levels

Use severity levels appropriately:
- **Info**: Normal operations, routine activities
- **Low**: Minor changes, non-critical updates
- **Medium**: Important operations, significant changes
- **High**: Critical operations, security-related events
- **Critical**: System-critical events requiring immediate attention

### Description Templates

Use clear, human-readable descriptions:
- ✅ `"User {Email} was created"`
- ✅ `"Order {OrderNumber} was placed by {CustomerName}"`
- ❌ `"User created"` (too vague)
- ❌ `"CRUD operation"` (not descriptive)

### Tags

Use consistent tag naming:
- Use lowercase with hyphens: `user-management`, `order-processing`
- Group related activities: `payment`, `payment-success`, `payment-failed`
- Avoid too many tags (3-5 tags maximum per log)

### Metadata

Store structured data in metadata:
- Use JSON-serializable types
- Keep metadata focused and relevant
- Avoid storing sensitive information (passwords, tokens, etc.)

### Performance Considerations

- **Async Operations**: Always use async methods
- **Batching**: Consider batching multiple logs when possible
- **Indexing**: Ensure database indexes on frequently queried fields (UserId, EntityId, ActivityType, CreationTime)
- **Cleanup**: Schedule periodic cleanup of old logs using `DeleteOldLogsAsync`

### Security

- **Sensitive Data**: Never log passwords, tokens, or other sensitive information
- **PII**: Be cautious when logging personally identifiable information (PII)
- **Public vs Private**: Use `isPublic: false` for internal-only logs
- **IP Addresses**: Be aware of GDPR/privacy regulations regarding IP logging

## 📝 Examples

### Example 1: User Management Activities

```csharp
public class UserService
{
    private readonly IActivityLogger _activityLogger;

    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        var user = new User { /* ... */ };
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        await _activityLogger.LogAsync(
            activityType: "UserCreated",
            description: $"User {dto.Email} was created",
            entityId: user.Id,
            entityType: "User",
            severity: ActivitySeverity.Info,
            tags: "user-management", "creation"
        );

        return user;
    }

    public async Task UpdateUserAsync(Guid userId, UpdateUserDto dto)
    {
        var user = await _repository.GetByIdAsync(userId);
        // ... update logic ...

        await _activityLogger.LogAsync(
            activityType: "UserUpdated",
            descriptionTemplateKey: "User {Email} profile was updated",
            descriptionParameters: new Dictionary<string, object>
            {
                ["Email"] = user.Email
            },
            entityId: user.Id,
            entityType: "User",
            metadata: new Dictionary<string, object>
            {
                ["ChangedFields"] = new[] { "FirstName", "LastName" }
            },
            severity: ActivitySeverity.Low,
            tags: "user-management", "update"
        );
    }
}
```

### Example 2: Order Processing Activities

```csharp
public class OrderService
{
    private readonly IActivityLogger _activityLogger;

    public async Task PlaceOrderAsync(Order order)
    {
        // ... order processing logic ...

        await _activityLogger.LogAsync(
            activityType: "OrderPlaced",
            descriptionTemplateKey: "Order {OrderNumber} for {Amount} was placed",
            descriptionParameters: new Dictionary<string, object>
            {
                ["OrderNumber"] = order.Number,
                ["Amount"] = order.TotalAmount
            },
            entityId: order.Id,
            entityType: "Order",
            metadata: new Dictionary<string, object>
            {
                ["ItemsCount"] = order.Items.Count,
                ["PaymentMethod"] = order.PaymentMethod
            },
            severity: ActivitySeverity.Medium,
            tags: "order", "purchase"
        );
    }

    public async Task ShipOrderAsync(Order order)
    {
        // ... shipping logic ...

        await _activityLogger.LogAsync(
            activityType: "OrderShipped",
            description: $"Order {order.Number} was shipped",
            entityId: order.Id,
            entityType: "Order",
            severity: ActivitySeverity.High,
            tags: "order", "fulfillment"
        );
    }
}
```

### Example 3: Security Events

```csharp
public class AuthService
{
    private readonly IActivityLogger _activityLogger;

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            await _activityLogger.LogAsync(
                activityType: "LoginFailed",
                description: $"Failed login attempt for {email}",
                userId: user?.Id,
                severity: ActivitySeverity.High,
                isPublic: false, // Security event - internal only
                tags: "security", "authentication"
            );
            return false;
        }

        await _activityLogger.LogAsync(
            activityType: "LoginSuccess",
            description: $"User {email} logged in successfully",
            userId: user.Id,
            severity: ActivitySeverity.Info,
            tags: "security", "authentication"
        );

        return true;
    }
}
```

### Example 4: Querying Activity Logs

```csharp
public class ActivityLogController
{
    private readonly IActivityLogStore _activityLogStore;

    [HttpGet]
    public async Task<IActionResult> GetActivities(
        int page = 1,
        int pageSize = 50,
        Guid? userId = null,
        string? activityType = null,
        ActivitySeverity? severity = null)
    {
        var (items, totalCount) = await _activityLogStore.GetListAsync(
            pageNumber: page,
            pageSize: pageSize,
            userId: userId,
            activityType: activityType,
            severity: severity
        );

        return Ok(new
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var statistics = await _activityLogStore.GetStatisticsAsync(
            startDate: startDate ?? DateTime.UtcNow.AddDays(-30),
            endDate: endDate ?? DateTime.UtcNow
        );

        return Ok(statistics);
    }
}
```

## 🔍 Troubleshooting

### Logs Not Appearing

1. **Check DI Registration**: Ensure `IActivityLogger` and `IActivityLogStore` are registered
2. **Check Database**: Verify migrations are applied
3. **Check Exceptions**: Logger silently fails - check application logs for exceptions
4. **Check HTTP Context**: If using `CentralizedActivityLogger`, ensure `IHttpContextAccessor` is registered

### Performance Issues

1. **Database Indexing**: Ensure indexes on `UserId`, `EntityId`, `ActivityType`, `CreationTime`
2. **Pagination**: Always use pagination for large datasets
3. **Cleanup**: Schedule periodic cleanup of old logs
4. **Async Operations**: Ensure all operations are async

### Missing Context Information

- **User Information**: Ensure user is authenticated and claims are set correctly
- **IP Address**: Check proxy configuration if behind load balancer
- **User Agent**: Verify request headers are available

## 📚 Additional Resources

- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Dependency Injection](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

---

**Happy Logging! 📊**

