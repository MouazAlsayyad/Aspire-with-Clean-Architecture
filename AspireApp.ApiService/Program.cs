using AspireApp.ApiService.Application.ActivityLogs.Services;
using AspireApp.ApiService.Application.Extensions;
using AspireApp.ApiService.Domain.ActivityLogs.Interfaces;
using AspireApp.ApiService.Domain.Authentication.Interfaces;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Extensions;
using AspireApp.ApiService.Presentation.Extensions;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.FirebaseNotifications.Application.UseCases;
using AspireApp.Email.Infrastructure.Extensions;
using AspireApp.Twilio.Infrastructure.Extensions;
using AspireApp.Notifications.Infrastructure.Extensions;
using AspireApp.Payment.Infrastructure.Extensions;
using AspireApp.Modules.FileUpload.Infrastructure.Extensions;
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

    // Register DbContext (base class) for module repositories that depend on it instead of ApplicationDbContext
    // This allows modules to remain independent of the concrete ApplicationDbContext type
    builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

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

    // Register FileUpload module services (strategies, factory, repository, domain manager)
    builder.Services.AddFileUploadServices(builder.Configuration);

    // Register Activity Logging services
    // Register IActivityLogStore manually (it doesn't follow the "Repository" naming convention)
    builder.Services.AddScoped<AspireApp.ApiService.Domain.ActivityLogs.Interfaces.IActivityLogStore, AspireApp.ApiService.Infrastructure.Repositories.ActivityLogRepository>();
    // IActivityLogger implementations are in Application layer, so we choose which one to use
    // TEMPORARILY DISABLED to prevent circular dependency in domain events
    // builder.Services.AddScoped<IActivityLogger, CentralizedActivityLogger>();

    // Register notification localization initializer
    builder.Services.AddHostedService<AspireApp.FirebaseNotifications.Infrastructure.Services.NotificationLocalizationInitializer>();

    // Register email service (SMTP or SendGrid based on configuration)
    builder.Services.AddEmailService(builder.Configuration);

    // Register Twilio service
    builder.Services.AddTwilioService(builder.Configuration);

    // Register notification strategies (must be before payment services)
    builder.Services.AddNotificationStrategies(builder.Configuration);

    // Register payment services
    builder.Services.AddPaymentServices(builder.Configuration);

    // Force load module assemblies to ensure they're available for service registration
    // This ensures the assemblies are loaded before AddUseCases(), AddAutoMapperConfiguration(), etc.
    _ = typeof(CreateNotificationUseCase).Assembly;
    _ = typeof(AspireApp.Twilio.Application.UseCases.SendWhatsAppUseCase).Assembly;
    _ = typeof(AspireApp.Email.Application.UseCases.SendBookingEmailUseCase).Assembly;
    _ = typeof(AspireApp.Notifications.Application.UseCases.SendNotificationUseCase).Assembly;
    _ = typeof(AspireApp.Payment.Application.UseCases.CreatePaymentUseCase).Assembly;
    _ = typeof(AspireApp.Modules.FileUpload.Application.UseCases.UploadFileUseCase).Assembly;

    // Register all use cases automatically from Application assembly
    builder.Services.AddUseCases();

    // Configure Authentication and Authorization (JWT + Roles + Permissions)
    builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

    // Configure OpenAPI/Swagger
    builder.Services.AddOpenApiConfiguration();

    var app = builder.Build();

    // Seed database
    Log.Information("Starting database seeding...");
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DatabaseSeeder.SeedAsync(context, passwordHasher);
    }
    Log.Information("Database seeding completed!");

    // Configure the HTTP request pipeline.
    Log.Information("Configuring HTTP request pipeline...");
    app.UseExceptionHandler();

    // HTTP request logging middleware (lightweight, using Serilog)
    // Set LogRequestBody and LogResponseBody to true only for debugging (impacts performance)
    var requestLoggingOptions = new AspireApp.ApiService.Infrastructure.Middleware.RequestLoggingOptions
    {
        SlowRequestThresholdMs = builder.Configuration.GetValue<int>("Logging:SlowRequestThresholdMs", 1000),
        MaxResponseBodyLength = builder.Configuration.GetValue<int>("Logging:MaxResponseBodyLength", 10000),
        ExcludedPaths = builder.Configuration.GetSection("Logging:ExcludedPaths").Get<List<string>>()
            ?? new List<string> { "/health", "/alive", "/metrics" },
        LogRequestBody = false,  // Set to true only for debugging
        LogResponseBody = false  // Set to true only for debugging
    };
    app.UseMiddleware<AspireApp.ApiService.Infrastructure.Middleware.RequestLoggingMiddleware>(requestLoggingOptions);

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
    Log.Information("Mapping endpoints...");
    app.MapEndpoints();

    Log.Information("Mapping default endpoints...");
    app.MapDefaultEndpoints();

    Log.Information("Starting web server...");
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

public partial class Program { }
