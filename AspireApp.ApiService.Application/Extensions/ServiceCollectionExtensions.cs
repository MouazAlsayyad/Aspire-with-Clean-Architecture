using AspireApp.ApiService.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace AspireApp.ApiService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Automatically registers all use cases from the Application assembly.
    /// Finds all classes that inherit from BaseUseCase OR end with "UseCase" and register them as scoped services.
    /// </summary>
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.GetAssembly(typeof(BaseUseCase));

        if (applicationAssembly == null)
            return services;

        // Find all use case implementations:
        // 1. Classes that inherit from BaseUseCase
        // 2. Classes that end with "UseCase" (for standalone use cases like LoginUserUseCase, RefreshTokenUseCase)
        var baseUseCaseTypes = applicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && 
                       !t.IsAbstract && 
                       t.IsSubclassOf(typeof(BaseUseCase)))
            .ToList();

        var standaloneUseCaseTypes = applicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && 
                       !t.IsAbstract && 
                       t.Name.EndsWith("UseCase") &&
                       !t.IsSubclassOf(typeof(BaseUseCase)) &&
                       t.Namespace != null &&
                       t.Namespace.Contains("UseCases"))
            .ToList();

        // Combine both lists and remove duplicates
        var allUseCaseTypes = baseUseCaseTypes
            .Union(standaloneUseCaseTypes)
            .ToList();

        // Register each use case as scoped service
        foreach (var useCaseType in allUseCaseTypes)
        {
            services.AddScoped(useCaseType);
        }

        return services;
    }
}
