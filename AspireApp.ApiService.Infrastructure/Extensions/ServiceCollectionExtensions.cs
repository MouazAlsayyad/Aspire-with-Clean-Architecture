using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.ApiService.Infrastructure.DomainEvents;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.ApiService.Infrastructure.Services;
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
            Assembly.GetAssembly(typeof(Domain.Roles.Entities.RolePermission)) // API Service Domain assembly
            // Module assemblies should be registered at the main application level
            // Assembly.GetAssembly(typeof(Modules.ActivityLogs.Domain.Entities.ActivityLog)), // ActivityLogs module
            // Assembly.GetAssembly(typeof(Modules.FileUpload.Domain.Entities.FileUpload)) // FileUpload module
        };
        
        // Dynamically load Notifications, Email, Twilio and Payment module assemblies to avoid circular dependency
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var notificationsDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.FirebaseNotifications");
        if (notificationsDomainAssembly != null)
        {
            domainAssembliesList.Add(notificationsDomainAssembly);
        }
        
        var emailDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        if (emailDomainAssembly != null)
        {
            domainAssembliesList.Add(emailDomainAssembly);
        }
        
        var twilioDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Twilio");
        if (twilioDomainAssembly != null)
        {
            domainAssembliesList.Add(twilioDomainAssembly);
        }
        
        var paymentDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Payment");
        if (paymentDomainAssembly != null)
        {
            domainAssembliesList.Add(paymentDomainAssembly);
        }
        
        var fileUploadDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Modules.FileUpload");
        if (fileUploadDomainAssembly != null)
        {
            domainAssembliesList.Add(fileUploadDomainAssembly);
        }
        
        var domainAssemblies = domainAssembliesList.ToArray();

        var infrastructureAssembliesList = new List<Assembly?>
        {
            Assembly.GetAssembly(typeof(Repository<>)) // Main Infrastructure assembly
        };
        
        // Also search in Notifications, Email, Twilio, Payment and FileUpload module assemblies for repository implementations
        var notificationsInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.FirebaseNotifications");
        if (notificationsInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(notificationsInfraAssembly);
        }
        
        var emailInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        if (emailInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(emailInfraAssembly);
        }
        
        var twilioInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Twilio");
        if (twilioInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(twilioInfraAssembly);
        }
        
        var paymentInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Payment");
        if (paymentInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(paymentInfraAssembly);
        }
        
        var fileUploadInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Modules.FileUpload");
        if (fileUploadInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(fileUploadInfraAssembly);
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
            Assembly.GetAssembly(typeof(DomainService)) // Main Domain assembly
            // Module assemblies should be registered at the main application level
            // Assembly.GetAssembly(typeof(AspireApp.Modules.FileUpload.Domain.Services.FileUploadManager)) // FileUpload module
        };
        
        // Dynamically load Notifications, Email, Twilio and Payment module assemblies to avoid circular dependency
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var notificationsDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.FirebaseNotifications");
        if (notificationsDomainAssembly != null)
        {
            domainAssembliesList.Add(notificationsDomainAssembly);
        }
        
        var emailDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        if (emailDomainAssembly != null)
        {
            domainAssembliesList.Add(emailDomainAssembly);
        }
        
        var twilioDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Twilio");
        if (twilioDomainAssembly != null)
        {
            domainAssembliesList.Add(twilioDomainAssembly);
        }
        
        var paymentDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Payment");
        if (paymentDomainAssembly != null)
        {
            domainAssembliesList.Add(paymentDomainAssembly);
        }
        
        var domainAssemblies = domainAssembliesList.ToArray();

        var infrastructureAssembliesList = new List<Assembly?>
        {
            Assembly.GetAssembly(typeof(Repository<>)) // Main Infrastructure assembly
        };
        
        // Also search in Notifications, Email, Twilio and Payment module assemblies for infrastructure implementations
        var notificationsInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.FirebaseNotifications");
        if (notificationsInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(notificationsInfraAssembly);
        }
        
        var emailInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        if (emailInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(emailInfraAssembly);
        }
        
        var twilioInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Twilio");
        if (twilioInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(twilioInfraAssembly);
        }
        
        var paymentInfraAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Payment");
        if (paymentInfraAssembly != null)
        {
            infrastructureAssembliesList.Add(paymentInfraAssembly);
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
    /// Searches both Infrastructure assembly and module assemblies (ActivityLogs, etc.).
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

        // Collect assemblies to search (Infrastructure + Module assemblies)
        var assembliesToSearch = new List<Assembly?> { infrastructureAssembly };

        // Dynamically load module assemblies to search for handlers
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var moduleAssemblyNames = new[]
        {
            "AspireApp.Modules.FileUpload",
            "AspireApp.FirebaseNotifications",
            "AspireApp.Email",
            "AspireApp.Twilio",
            "AspireApp.Payment"
        };

        foreach (var assemblyName in moduleAssemblyNames)
        {
            var moduleAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (moduleAssembly != null)
            {
                assembliesToSearch.Add(moduleAssembly);
            }
        }

        // Find all types that implement IDomainEventHandler<T> across all assemblies
        var handlerTypes = new List<(Type HandlerType, Type EventType)>();
        
        foreach (var assembly in assembliesToSearch)
        {
            if (assembly == null)
                continue;

            var handlersInAssembly = assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                               i.GetGenericTypeDefinition() == handlerInterfaceGenericDefinition)
                    .Select(i => (HandlerType: t, EventType: i.GetGenericArguments()[0])))
                .ToList();

            handlerTypes.AddRange(handlersInAssembly);
        }

        // Register each handler with its event type
        foreach (var (handlerType, eventType) in handlerTypes)
        {
            var handlerInterfaceTypeForEvent = handlerInterfaceGenericDefinition.MakeGenericType(eventType);
            services.AddScoped(handlerInterfaceTypeForEvent, handlerType);
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
            Assembly.GetAssembly(typeof(IPasswordHasher)) // Main Domain assembly
            // Module assemblies should be registered at the main application level
            // Assembly.GetAssembly(typeof(AspireApp.Modules.ActivityLogs.Domain.Interfaces.IActivityLogStore)), // ActivityLogs module
            // Assembly.GetAssembly(typeof(AspireApp.Modules.FileUpload.Domain.Interfaces.IFileUploadRepository)) // FileUpload module
        };
        
        // Dynamically load Notifications module assembly to avoid circular dependency
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var notificationsDomainAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.FirebaseNotifications");
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
            "AspireApp.Modules.FileUpload.Domain",
            "AspireApp.ApiService.Notifications.Domain",
            "AspireApp.Twilio.Domain"
        };
        
        // Exclude certain interfaces from automatic registration
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
                           // t != typeof(IFileStorageStrategy) && // File storage types - requires FileUpload module
                           // t != typeof(IFileStorageStrategyFactory) && // File storage types - requires FileUpload module
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
    /// NOTE: This method depends on FileUpload module types and should be called from the main application layer
    /// </summary>
    [Obsolete("Use AddFileUploadServices from AspireApp.Modules.FileUpload.Infrastructure.Extensions instead. This method is empty and does nothing.")]
    public static IServiceCollection AddFileStorageStrategies(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        // This method has been replaced by AddFileUploadServices in the FileUpload module.
        // Please use: builder.Services.AddFileUploadServices(builder.Configuration);
        // from AspireApp.Modules.FileUpload.Infrastructure.Extensions namespace
        
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

