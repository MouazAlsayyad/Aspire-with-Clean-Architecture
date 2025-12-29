using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Mappings;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.ApiService.Infrastructure.DomainEvents;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.ApiService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
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
        var domainAssembly = Assembly.GetAssembly(typeof(BaseEntity));
        var infrastructureAssembly = Assembly.GetAssembly(typeof(Repository<>));

        if (domainAssembly == null || infrastructureAssembly == null)
            return services;

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
            var implementationType = infrastructureAssembly
                .GetTypes()
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.Name == implementationName && repositoryInterface.IsAssignableFrom(t));

            if (implementationType != null)
            {
                services.AddScoped(repositoryInterface, implementationType);
            }
        }

        return services;
    }

    /// <summary>
    /// Automatically registers all domain managers (domain services).
    /// Finds all classes that inherit from DomainService and register them.
    /// </summary>
    public static IServiceCollection AddDomainManagers(this IServiceCollection services)
    {
        var domainAssembly = Assembly.GetAssembly(typeof(DomainService));

        if (domainAssembly == null)
            return services;

        // Find all domain service implementations (classes that inherit from DomainService)
        var domainServiceTypes = domainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DomainService)))
            .ToList();

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
        var domainAssembly = Assembly.GetAssembly(typeof(IPasswordHasher));
        var infrastructureAssembly = Assembly.GetAssembly(typeof(PasswordHasher));

        if (domainAssembly == null || infrastructureAssembly == null)
            return services;

        // Register Unit of Work
        services.AddUnitOfWork();

        // Register HTTP Context Accessor
        services.AddHttpContextAccessorService();

        // Register Domain Event Dispatcher
        services.AddDomainEventDispatcher();

        // Register Domain Event Handlers
        services.AddDomainEventHandlers();

        // Get all interfaces from Domain.Interfaces namespace
        var domainInterfaces = domainAssembly
            .GetTypes()
            .Where(t => t.IsInterface && 
                       t.Namespace == typeof(IPasswordHasher).Namespace &&
                       t != typeof(IDomainEventDispatcher) && // Skip IDomainEventDispatcher (already registered)
                       !t.Name.StartsWith("IRepository") && // Skip repositories (registered by AddRepositories)
                       !t.Name.StartsWith("IDomainService")) // Skip domain services (registered by AddDomainManagers)
            .ToList();

        // Find implementations in Infrastructure assembly
        foreach (var interfaceType in domainInterfaces)
        {
            // Find implementation class that implements this interface
            var implementationType = infrastructureAssembly
                .GetTypes()
                .FirstOrDefault(t => t.IsClass && 
                                    !t.IsAbstract && 
                                    interfaceType.IsAssignableFrom(t) &&
                                    !t.IsGenericType);

            if (implementationType != null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }

        return services;
    }
}

