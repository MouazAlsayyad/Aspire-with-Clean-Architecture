using AspireApp.ApiService.Application.Extensions;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Extensions;
using AspireApp.ApiService.Presentation.Extensions;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.ActivityLogs.Application.Services;
using AspireApp.Modules.ActivityLogs.Domain.Interfaces;
using AspireApp.ApiService.Notifications.Application.UseCases;
using AspireApp.Email.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text.RegularExpressions;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/logs-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/logs-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        formatter: new Serilog.Formatting.Json.JsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Starting AspireApp API Service");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add service defaults & Aspire client integrations.
    builder.AddServiceDefaults();

    // Add services to the container.
    builder.Services.AddProblemDetails();

    // Configure Entity Framework Core with SQL Server
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    {
        options.UseSqlServer(connectionString);
    }, ServiceLifetime.Scoped);

    // Register DbContext factory with domain event dispatcher
    builder.Services.AddScoped<ApplicationDbContext>(sp =>
    {
        var options = sp.GetRequiredService<Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>>();
        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();
        return new ApplicationDbContext(options, dispatcher);
    });

    // Register all repositories automatically (generic and specific)
    builder.Services.AddRepositories();

    // Register domain managers (domain services)
    builder.Services.AddDomainManagers();

    // Configure AutoMapper
    builder.Services.AddAutoMapperConfiguration();

    // Configure FluentValidation
    builder.Services.AddFluentValidationConfiguration();

    // Register infrastructure services automatically (includes UnitOfWork, HttpContextAccessor, DomainEventDispatcher, DomainEventHandlers, and infrastructure services)
    builder.Services.AddInfrastructureServices();

    // Register background task queue and hosted service
    builder.Services.AddBackgroundTaskQueue();

    // Register file storage strategies
    builder.Services.AddFileStorageStrategies(builder.Configuration);

    // Register Activity Logging services
    // Note: IActivityLogStore is registered automatically via AddInfrastructureServices()
    // IActivityLogger implementations are in Application layer, so we choose which one to use
    builder.Services.AddScoped<IActivityLogger, CentralizedActivityLogger>();

    // Register notification localization initializer
    builder.Services.AddHostedService<AspireApp.ApiService.Notifications.Infrastructure.Services.NotificationLocalizationInitializer>();

    // Register email service (SMTP or SendGrid based on configuration)
    builder.Services.AddEmailService(builder.Configuration);

    // Force load module assemblies to ensure they're available for service registration
    // This ensures the assemblies are loaded before AddUseCases(), AddAutoMapperConfiguration(), etc.
    _ = typeof(CreateNotificationUseCase).Assembly;
    _ = typeof(AspireApp.Twilio.Application.UseCases.SendWhatsAppUseCase).Assembly;
    _ = typeof(AspireApp.Email.Application.UseCases.SendBookingEmailUseCase).Assembly;
    
    // Register all use cases automatically from Application assembly
    builder.Services.AddUseCases();
    
    // DIAGNOSTIC: Verify use case registration
    Log.Information("========== VERIFYING USE CASE REGISTRATION ==========");
    var tempProvider = builder.Services.BuildServiceProvider();
    try
    {
        var loginUseCase = tempProvider.GetService<AspireApp.ApiService.Application.Authentication.UseCases.LoginUserUseCase>();
        Log.Information($"✅ LoginUserUseCase resolved: {(loginUseCase != null ? "SUCCESS" : "FAILED")}");
        
        var createNotificationUseCase = tempProvider.GetService<CreateNotificationUseCase>();
        Log.Information($"✅ CreateNotificationUseCase resolved: {(createNotificationUseCase != null ? "SUCCESS" : "FAILED")}");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Error resolving Use Cases");
    }
    Log.Information("====================================================");

    // Configure Authentication and Authorization (JWT + Roles + Permissions)
    builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

    // Configure OpenAPI/Swagger
    builder.Services.AddOpenApiConfiguration();

    var app = builder.Build();

    // Seed database
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DatabaseSeeder.SeedAsync(context, passwordHasher);
    }

    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();

    // HTTP request logging middleware removed for performance - it was causing significant slowdowns
    // Uncomment below to re-enable if needed:
    // var requestLoggingOptions = new RequestLoggingOptions
    // {
    //     SlowRequestThresholdMs = builder.Configuration.GetValue<int>("Logging:SlowRequestThresholdMs", 1000),
    //     MaxResponseBodyLength = builder.Configuration.GetValue<int>("Logging:MaxResponseBodyLength", 10000),
    //     ExcludedPaths = builder.Configuration.GetSection("Logging:ExcludedPaths").Get<List<string>>() 
    //         ?? new List<string> { "/health", "/alive", "/metrics" }
    // };
    // app.UseMiddleware<RequestLoggingMiddleware>(requestLoggingOptions);

    // Add authentication & authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        // Configure Scalar UI with JWT authentication support
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("AspireApp API - Authentication & RBAC")
                .WithTheme(ScalarTheme.Purple)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    // Redirect root path to Scalar UI dynamically
    // Extract version from OpenAPI route pattern (default is "v1" for MapOpenApi())
    app.MapGet("/", (HttpContext context) =>
    {
        var openApiVersion = "v1"; // Default version

        // Try to extract version from OpenAPI route by checking route data
        // MapOpenApi() creates route like "/openapi/v1.json", Scalar uses "/scalar/v1"
        var endpointDataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            // Check display name for route pattern
            var displayName = endpoint.DisplayName ?? string.Empty;
            var versionMatch = Regex.Match(displayName, @"/openapi/(v\d+)");
            if (versionMatch.Success)
            {
                openApiVersion = versionMatch.Groups[1].Value;
                break;
            }

            // Check route template from metadata
            var routeNameMetadata = endpoint.Metadata.GetMetadata<RouteNameMetadata>();
            if (routeNameMetadata?.RouteName != null)
            {
                var match = Regex.Match(routeNameMetadata.RouteName, @"(v\d+)");
                if (match.Success)
                {
                    openApiVersion = match.Groups[1].Value;
                    break;
                }
            }
        }

        return Results.Redirect($"/scalar/{openApiVersion}", permanent: false);
    })
    .ExcludeFromDescription();

    // Map all endpoints automatically
    app.MapEndpoints();

    app.MapDefaultEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
