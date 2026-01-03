using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AspireApp.ApiService.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Force load module assemblies for design-time (migrations)
        // This ensures EF Core can discover entity configurations from these assemblies
        ForceLoadModuleAssemblies();

        // Try multiple paths to find appsettings.json
        var basePath = FindAppSettingsPath();

        // Build configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static void ForceLoadModuleAssemblies()
    {
        try
        {
            // Get the Infrastructure project directory
            var infrastructureDir = Directory.GetCurrentDirectory();
            var binPath = Path.Combine(infrastructureDir, "bin", "Debug", "net10.0");

            // If we're not in the bin directory, try to find it
            if (!Directory.Exists(binPath))
            {
                // Try alternative paths
                binPath = Path.Combine(infrastructureDir, "..", "AspireApp.ApiService.Infrastructure", "bin", "Debug", "net10.0");
            }

            if (Directory.Exists(binPath))
            {
                // Load module assemblies explicitly
                var moduleAssemblies = new[]
                {
                    "AspireApp.Modules.FileUpload.dll",
                    "AspireApp.FirebaseNotifications.dll",
                    "AspireApp.Email.dll",
                    "AspireApp.Twilio.dll",
                    "AspireApp.Payment.dll"
                };

                foreach (var assemblyName in moduleAssemblies)
                {
                    var assemblyPath = Path.Combine(binPath, assemblyName);
                    if (File.Exists(assemblyPath))
                    {
                        Assembly.LoadFrom(assemblyPath);
                    }
                }
            }
        }
        catch
        {
            // Ignore errors - assemblies might already be loaded or not needed for this migration
        }
    }

    private static string FindAppSettingsPath()
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Try current directory
        if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            return currentDir;
        }

        // Try going up one level to ApiService project
        var apiServicePath = Path.Combine(currentDir, "..", "AspireApp.ApiService");
        if (Directory.Exists(apiServicePath) && File.Exists(Path.Combine(apiServicePath, "appsettings.json")))
        {
            return apiServicePath;
        }

        // Try going up two levels from Infrastructure project
        var infrastructurePath = Path.GetDirectoryName(currentDir);
        if (infrastructurePath != null)
        {
            apiServicePath = Path.Combine(infrastructurePath, "..", "AspireApp.ApiService");
            if (Directory.Exists(apiServicePath) && File.Exists(Path.Combine(apiServicePath, "appsettings.json")))
            {
                return apiServicePath;
            }
        }

        throw new InvalidOperationException("Could not find appsettings.json file. Please ensure it exists in the AspireApp.ApiService project directory.");
    }
}

// Add-Migration Init -StartupProject AspireApp.ApiService
// Update-Database -StartupProject AspireApp.ApiService