using AspireApp.ApiService.Application.Mappings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.ApiService.Application.Extensions;

/// <summary>
/// Extension methods for configuring Application layer services
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Configures AutoMapper with mapping profiles from Application assembly
    /// </summary>
    public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(PermissionMappingProfile).Assembly);
        return services;
    }

    /// <summary>
    /// Configures FluentValidation with validators from Application assembly
    /// </summary>
    public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<PermissionMappingProfile>();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        return services;
    }
}

