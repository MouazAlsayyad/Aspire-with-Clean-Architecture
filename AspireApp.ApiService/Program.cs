using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Infrastructure.Authorization;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Extensions;
using AspireApp.ApiService.Infrastructure.Identity;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.ApiService.Infrastructure.Services;
using AspireApp.ApiService.Presentation.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.RegularExpressions;
using AspireApp.ApiService.Application.Mappings;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Configure Entity Framework Core with SQL Server
var connectionString = builder.Configuration.GetConnectionString("Default") 
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register all repositories automatically (generic and specific)
builder.Services.AddRepositories();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register domain managers (domain services)
builder.Services.AddDomainManagers();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(PermissionMappingProfile).Assembly);

// Configure FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PermissionMappingProfile>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Register infrastructure services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Register all use cases automatically from Application assembly
builder.Services.AddUseCases();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? 
    throw new InvalidOperationException("Jwt:SecretKey is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AspireApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AspireApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Skip challenge for anonymous endpoints
            if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                context.HandleResponse();
            }
            return Task.CompletedTask;
        }
    };
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"))
    .AddPolicy("UserOrAbove", policy => policy.RequireRole("User", "Manager", "Admin"));

// Register custom authorization policy provider for permissions
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Endpoints API Explorer for OpenAPI
builder.Services.AddEndpointsApiExplorer();

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
});

// Map endpoints
app.MapAuthEndpoints();
app.MapWeatherEndpoints();
app.MapRoleEndpoints();
app.MapPermissionEndpoints();
app.MapUserEndpoints();

app.MapDefaultEndpoints();

app.Run();
