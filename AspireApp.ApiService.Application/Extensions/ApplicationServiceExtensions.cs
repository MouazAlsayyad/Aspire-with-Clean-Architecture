using AspireApp.ApiService.Application.Permissions.Mappings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AspireApp.ApiService.Application.Extensions;

/// <summary>
/// Extension methods for configuring Application layer services
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Configures AutoMapper with mapping profiles from Application assembly and module assemblies
    /// </summary>
    public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
    {
        // Scan main Application assembly
        services.AddAutoMapper(typeof(PermissionMappingProfile).Assembly);
        
        // Scan module assemblies dynamically to avoid circular dependencies
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var activityLogsAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Modules.ActivityLogs");
        var fileUploadAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Modules.FileUpload");
        var notificationsAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        var twilioAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Twilio");
        var emailAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        
        if (activityLogsAssembly != null)
        {
            services.AddAutoMapper(activityLogsAssembly);
        }
        
        if (fileUploadAssembly != null)
        {
            services.AddAutoMapper(fileUploadAssembly);
        }
        
        if (notificationsAssembly != null)
        {
            services.AddAutoMapper(notificationsAssembly);
        }
        
        if (twilioAssembly != null)
        {
            services.AddAutoMapper(twilioAssembly);
        }
        
        if (emailAssembly != null)
        {
            services.AddAutoMapper(emailAssembly);
        }
        
        return services;
    }

    /// <summary>
    /// Configures FluentValidation with validators from Application assembly and module assemblies
    /// </summary>
    public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
    {
        // Scan main Application assembly
        services.AddValidatorsFromAssemblyContaining<PermissionMappingProfile>();
        
        // Scan module assemblies dynamically to avoid circular dependencies
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var activityLogsAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Modules.ActivityLogs");
        var fileUploadAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Modules.FileUpload");
        var notificationsAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.ApiService.Notifications");
        var twilioAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Twilio");
        var emailAssembly = loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "AspireApp.Email");
        
        if (activityLogsAssembly != null)
        {
            services.AddValidatorsFromAssembly(activityLogsAssembly);
        }
        
        if (fileUploadAssembly != null)
        {
            services.AddValidatorsFromAssembly(fileUploadAssembly);
        }
        
        if (notificationsAssembly != null)
        {
            services.AddValidatorsFromAssembly(notificationsAssembly);
        }
        
        if (twilioAssembly != null)
        {
            services.AddValidatorsFromAssembly(twilioAssembly);
        }
        
        if (emailAssembly != null)
        {
            services.AddValidatorsFromAssembly(emailAssembly);
        }
        
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        return services;
    }
}

