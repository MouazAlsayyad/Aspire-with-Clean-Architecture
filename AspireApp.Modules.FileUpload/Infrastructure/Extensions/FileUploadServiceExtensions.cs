using AspireApp.Modules.FileUpload.Domain.Interfaces;
using AspireApp.Modules.FileUpload.Domain.Services;
using AspireApp.Modules.FileUpload.Infrastructure.Repositories;
using AspireApp.Modules.FileUpload.Infrastructure.Services.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Modules.FileUpload.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering FileUpload module services
/// </summary>
public static class FileUploadServiceExtensions
{
    /// <summary>
    /// Registers all FileUpload module services including storage strategies, factory, repository, and domain manager
    /// </summary>
    public static IServiceCollection AddFileUploadServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register all file storage strategy implementations
        // All strategies must be registered as IFileStorageStrategy for the factory to discover them
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

        // Register repository
        services.AddScoped<IFileUploadRepository, FileUploadRepository>();

        // Register domain manager
        services.AddScoped<IFileUploadManager, FileUploadManager>();

        return services;
    }
}

