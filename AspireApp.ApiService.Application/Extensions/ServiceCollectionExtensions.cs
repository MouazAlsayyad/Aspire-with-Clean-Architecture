using AspireApp.Domain.Shared.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AspireApp.ApiService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Automatically registers all use cases from the Application assembly and module assemblies.
    /// Finds all classes that inherit from BaseUseCase OR end with "UseCase" and register them as scoped services.
    /// </summary>
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        // Get main Application assembly by using a marker type from this assembly
        // Cannot use GetExecutingAssembly() because extension methods execute in the caller's context
        var mainAssembly = typeof(ServiceCollectionExtensions).Assembly;
        var assemblies = new List<Assembly> { mainAssembly };
        
        // Discover module assemblies dynamically to avoid circular dependencies
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var activityLogsAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Modules.ActivityLogs");
        var fileUploadAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Modules.FileUpload");
        var notificationsAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        var twilioAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Twilio");
        
        if (activityLogsAssembly != null)
        {
            assemblies.Add(activityLogsAssembly);
        }
        
        if (fileUploadAssembly != null)
        {
            assemblies.Add(fileUploadAssembly);
        }
        
        if (notificationsAssembly != null)
        {
            assemblies.Add(notificationsAssembly);
        }
        
        if (twilioAssembly != null)
        {
            assemblies.Add(twilioAssembly);
        }

        foreach (var applicationAssembly in assemblies)
        {
            if (applicationAssembly == null)
                continue;

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
        }

        return services;
    }
}
