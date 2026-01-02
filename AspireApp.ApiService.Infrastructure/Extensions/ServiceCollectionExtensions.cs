using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.ApiService.Infrastructure.DomainEvents;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.ApiService.Infrastructure.Services;
using AspireApp.Modules.FileUpload.Infrastructure.Services.FileStorage;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AspireApp.ApiService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Automatically registers all generic repositories for entities that inherit from BaseEntity
    /// and all specific repository interfaces with their implementations
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var domainAssembliesList = new List<Assembly?>
        {
            Assembly.GetAssembly(typeof(BaseEntity)), // Main Domain assembly
            Assembly.GetAssembly(typeof(Domain.Roles.Entities.RolePermission)), // API Service Domain assembly
            Assembly.GetAssembly(typeof(Modules.ActivityLogs.Domain.Entities.ActivityLog)), // ActivityLogs module
            Assembly.GetAssembly(typeof(Modules.FileUpload.Domain.Entities.FileUpload)), // FileUpload module
            Assembly.GetAssembly(typeof(Twilio.Domain.Entities.Message)) // Twilio module
        };
        
        // Dynamically load Notifications and Email module assemblies to avoid circular dependency
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var notificationsDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        if (notificationsDomainAssembly != null)
        {
            domainAssembliesList.Add(notificationsDomainAssembly);
        }
        
        var emailDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        if (emailDomainAssembly != null)
        {
            domainAssembliesList.Add(emailDomainAssembly);
        }
        
        var domainAssemblies = domainAssembliesList.ToArray();

        var infrastructureAssembliesList = new List<Assembly?>
        {
            Assembly.GetAssembly(typeof(Repository<>)) // Main Infrastructure assembly (includes Twilio repositories)
        };
        
        // Also search in Notifications and Email module assemblies for repository implementations
        var notificationsInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        if (notificationsInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(notificationsInfraAssembly);
        }
        
        var emailInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        if (emailInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(emailInfraAssembly);
        }
        
        var infrastructureAssemblies = infrastructureAssembliesList.ToArray();

        foreach (var domainAssembly in domainAssemblies)
        {
            if (domainAssembly == null)
                continue;

            // Register generic repositories: IRepository<T> -> Repository<T> for each entity type
            var entityTypes = domainAssembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)))
                .ToList();

            foreach (var entityType in entityTypes)
            {
                var repositoryInterface = typeof(IRepository<>).MakeGenericType(entityType);
                var repositoryImplementation = typeof(Repository<>).MakeGenericType(entityType);

                services.AddScoped(repositoryInterface, repositoryImplementation);
            }

            // Register specific repositories: I*Repository -> *Repository
            var repositoryInterfaces = domainAssembly
                .GetTypes()
                .Where(t => t.IsInterface && t.Name.EndsWith("Repository") && t.Name.StartsWith("I"))
                .ToList();

            foreach (var repositoryInterface in repositoryInterfaces)
            {
                // Find the corresponding implementation class (remove the 'I' prefix)
                var implementationName = repositoryInterface.Name.Substring(1); // Remove 'I' prefix
                
                // Search in all infrastructure assemblies
                Type? implementationType = null;
                foreach (var infrastructureAssembly in infrastructureAssemblies)
                {
                    if (infrastructureAssembly == null)
                        continue;
                    
                    implementationType = infrastructureAssembly
                        .GetTypes()
                        .FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.Name == implementationName && repositoryInterface.IsAssignableFrom(t));
                    
                    if (implementationType != null)
                        break;
                }

                if (implementationType != null)
                {
                    services.AddScoped(repositoryInterface, implementationType);
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Automatically registers all domain managers (domain services).
    /// Finds all classes that inherit from DomainService and register them.
    /// Searches both Domain and Infrastructure assemblies, including module assemblies.
    /// </summary>
    public static IServiceCollection AddDomainManagers(this IServiceCollection services)
    {
        var domainAssembliesList = new List<Assembly?>
        {
            Assembly.GetAssembly(typeof(DomainService)), // Main Domain assembly
            Assembly.GetAssembly(typeof(AspireApp.Modules.FileUpload.Domain.Services.FileUploadManager)), // FileUpload module
            Assembly.GetAssembly(typeof(AspireApp.Twilio.Domain.Services.TwilioSmsManager)) // Twilio module
        };
        
        // Dynamically load Notifications module assembly to avoid circular dependency
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var notificationsDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        if (notificationsDomainAssembly != null)
        {
            domainAssembliesList.Add(notificationsDomainAssembly);
        }
        
        var domainAssemblies = domainAssembliesList.ToArray();

        var infrastructureAssembliesList = new List<Assembly?>
        {
            Assembly.GetAssembly(typeof(Repository<>)) // Main Infrastructure assembly (includes Twilio services)
        };
        
        // Also search in Notifications module assembly for infrastructure implementations
        var notificationsInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        if (notificationsInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(notificationsInfraAssembly);
        }
        
        var infrastructureAssemblies = infrastructureAssembliesList.ToArray();

        var domainServiceTypes = new List<Type>();

        // Find all domain service implementations in Domain assemblies
        foreach (var domainAssembly in domainAssemblies)
        {
            if (domainAssembly == null)
                continue;

            var domainServiceTypesFromDomain = domainAssembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DomainService)))
                .ToList();
            domainServiceTypes.AddRange(domainServiceTypesFromDomain);
        }

        // Also find domain service implementations in Infrastructure assemblies
        // (e.g., FirebaseNotificationManager which is in Infrastructure but inherits from DomainService)
        foreach (var infrastructureAssembly in infrastructureAssemblies)
        {
            if (infrastructureAssembly == null)
                continue;

            var domainServiceTypesFromInfrastructure = infrastructureAssembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DomainService)))
                .ToList();
            domainServiceTypes.AddRange(domainServiceTypesFromInfrastructure);
        }

        // Register each domain service
        foreach (var domainServiceType in domainServiceTypes)
        {
            services.AddScoped(domainServiceType);

            // Find and register interfaces that inherit from IDomainService (e.g., IUserManager)
            var interfaces = domainServiceType.GetInterfaces()
                .Where(i => i.IsInterface &&
                           i.Name.StartsWith("I") &&
                           typeof(IDomainService).IsAssignableFrom(i) &&
                           i != typeof(IDomainService))
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, domainServiceType);
            }
        }

        // Also register as IDomainService if they implement it
        foreach (var domainServiceType in domainServiceTypes)
        {
            if (typeof(IDomainService).IsAssignableFrom(domainServiceType) && domainServiceType != typeof(DomainService))
            {
                services.AddScoped(typeof(IDomainService), domainServiceType);
            }
        }

        return services;
    }

    /// <summary>
    /// Automatically registers all domain event handlers.
    /// Finds all classes that implement IDomainEventHandler&lt;T&gt; and registers them.
    /// </summary>
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services)
    {
        var infrastructureAssembly = Assembly.GetAssembly(typeof(DomainEventDispatcher));

        if (infrastructureAssembly == null)
            return services;

        // Get the IDomainEventHandler<T> type from the DomainEvents namespace
        var handlerInterfaceType = infrastructureAssembly
            .GetTypes()
            .FirstOrDefault(t => t.IsGenericType &&
                                t.Name == "IDomainEventHandler`1" &&
                                t.Namespace == typeof(DomainEventDispatcher).Namespace);

        if (handlerInterfaceType == null)
            return services;

        var handlerInterfaceGenericDefinition = handlerInterfaceType.GetGenericTypeDefinition();

        // Find all types that implement IDomainEventHandler<T>
        var handlerTypes = infrastructureAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == handlerInterfaceGenericDefinition)
                .Select(i => new { HandlerType = t, EventType = i.GetGenericArguments()[0] }))
            .ToList();

        // Register each handler with its event type
        foreach (var handler in handlerTypes)
        {
            var handlerInterfaceTypeForEvent = handlerInterfaceGenericDefinition.MakeGenericType(handler.EventType);
            services.AddScoped(handlerInterfaceTypeForEvent, handler.HandlerType);
        }

        return services;
    }

    /// <summary>
    /// Registers Unit of Work pattern implementation
    /// </summary>
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    /// <summary>
    /// Registers HTTP Context Accessor (required for services that need HTTP context)
    /// </summary>
    public static IServiceCollection AddHttpContextAccessorService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        return services;
    }

    /// <summary>
    /// Registers Domain Event Dispatcher for DDD pattern
    /// </summary>
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }

    /// <summary>
    /// Registers all infrastructure-related services in one call.
    /// This includes: Unit of Work, HTTP Context Accessor, Domain Event Dispatcher, and Domain Event Handlers.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        var domainAssembliesList = new List<Assembly?>
        {
            Assembly.GetAssembly(typeof(IPasswordHasher)), // Main Domain assembly
            Assembly.GetAssembly(typeof(AspireApp.Modules.ActivityLogs.Domain.Interfaces.IActivityLogStore)), // ActivityLogs module
            Assembly.GetAssembly(typeof(AspireApp.Modules.FileUpload.Domain.Interfaces.IFileUploadRepository)), // FileUpload module
            Assembly.GetAssembly(typeof(AspireApp.Twilio.Domain.Interfaces.IMessageRepository)) // Twilio module
        };
        
        // Dynamically load Notifications module assembly to avoid circular dependency
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var notificationsDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        if (notificationsDomainAssembly != null)
        {
            domainAssembliesList.Add(notificationsDomainAssembly);
        }
        
        var domainAssemblies = domainAssembliesList.ToArray();

        var infrastructureAssemblies = new[]
        {
            Assembly.GetAssembly(typeof(PasswordHasher)) // Main Infrastructure assembly
        };

        if (domainAssemblies[0] == null || infrastructureAssemblies[0] == null)
            return services;

        // Register Unit of Work
        services.AddUnitOfWork();

        // Register HTTP Context Accessor
        services.AddHttpContextAccessorService();

        // Register Domain Event Dispatcher
        services.AddDomainEventDispatcher();

        // Register Domain Event Handlers
        services.AddDomainEventHandlers();

        // Get all interfaces from Domain assemblies (all namespaces under Domain and Modules)
        var domainBaseNamespaces = new[]
        {
            "AspireApp.ApiService.Domain",
            "AspireApp.Modules.ActivityLogs.Domain",
            "AspireApp.Modules.FileUpload.Domain",
            "AspireApp.ApiService.Notifications.Domain",
            "AspireApp.Twilio.Domain"
        };
        
        // Exclude FileUpload and Notification interfaces from API Service Domain (use module versions instead)
        var excludedInterfaces = new[]
        {
            "AspireApp.ApiService.Domain.FileUploads.Interfaces.IFileStorageStrategy",
            "AspireApp.ApiService.Domain.FileUploads.Interfaces.IFileStorageStrategyFactory",
            "AspireApp.ApiService.Domain.FileUploads.Interfaces.IFileUploadRepository",
            "AspireApp.ApiService.Domain.Notifications.Interfaces.INotificationRepository",
            "AspireApp.ApiService.Domain.Notifications.Interfaces.INotificationManager"
        };

        var domainInterfaces = new List<Type>();
        foreach (var domainAssembly in domainAssemblies)
        {
            if (domainAssembly == null)
                continue;

            var interfaces = domainAssembly
                .GetTypes()
                .Where(t => t.IsInterface &&
                           domainBaseNamespaces.Any(ns => t.Namespace?.StartsWith(ns) == true) &&
                           !excludedInterfaces.Contains(t.FullName) && // Skip excluded interfaces
                           t != typeof(IDomainEventDispatcher) && // Skip IDomainEventDispatcher (already registered)
                           !t.Name.StartsWith("IRepository") && // Skip repositories (registered by AddRepositories)
                           !t.Name.StartsWith("IDomainService") && // Skip domain services (registered by AddDomainManagers)
                           t != typeof(IFileStorageStrategy) && // Skip file storage strategies (registered by AddFileStorageStrategies)
                           t != typeof(IFileStorageStrategyFactory) && // Skip file storage strategy factory (registered by AddFileStorageStrategies)
                           t != typeof(IBackgroundTaskQueue)) // Skip background task queue (registered by AddBackgroundTaskQueue)
                .ToList();
            
            domainInterfaces.AddRange(interfaces);
        }

        // Find implementations in Infrastructure assemblies
        foreach (var interfaceType in domainInterfaces)
        {
            // Find implementation class that implements this interface
            Type? implementationType = null;
            foreach (var infrastructureAssembly in infrastructureAssemblies)
            {
                if (infrastructureAssembly == null)
                    continue;

                implementationType = infrastructureAssembly
                    .GetTypes()
                    .FirstOrDefault(t => t.IsClass &&
                                        !t.IsAbstract &&
                                        interfaceType.IsAssignableFrom(t) &&
                                        !t.IsGenericType);

                if (implementationType != null)
                    break;
            }

            if (implementationType != null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }


        return services;
    }

    /// <summary>
    /// Registers file storage strategies and factory
    /// </summary>
    public static IServiceCollection AddFileStorageStrategies(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        // Register all file storage strategy implementations
        services.AddScoped<IFileStorageStrategy, FileSystemStorageStrategy>();
        services.AddScoped<IFileStorageStrategy, DatabaseStorageStrategy>();

        // Only register R2 if it's configured with a non-dummy AccountId
        // WARNING: Cloudflare R2 implementation is not fully tested or complete.
        // Use with caution and ensure thorough testing before production deployment.
        var r2AccountId = configuration["FileStorage:R2:AccountId"];
        if (!string.IsNullOrWhiteSpace(r2AccountId) && r2AccountId != "your-account-id")
        {
            services.AddScoped<IFileStorageStrategy, R2StorageStrategy>();
        }

        // Register factory with interface
        services.AddScoped<IFileStorageStrategyFactory, FileStorageStrategyFactory>();

        return services;
    }

    /// <summary>
    /// Registers background task queue and hosted service for processing queued background tasks.
    /// This provides a structured, scalable, and production-friendly approach to background task processing
    /// with graceful shutdown support and proper lifecycle management.
    /// </summary>
    public static IServiceCollection AddBackgroundTaskQueue(this IServiceCollection services)
    {
        // Register the queue as singleton since it's shared across the application
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

        // Register the hosted service that processes queued tasks
        services.AddHostedService<QueuedHostedService>();

        return services;
    }
}

