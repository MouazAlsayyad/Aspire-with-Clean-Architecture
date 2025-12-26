using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AspireApp.ApiService.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
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