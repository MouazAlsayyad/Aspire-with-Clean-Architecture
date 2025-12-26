using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.ApiService.Infrastructure.Repositories;
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
}

