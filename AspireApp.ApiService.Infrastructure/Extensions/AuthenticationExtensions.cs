using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AspireApp.ApiService.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring authentication and authorization
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Configures JWT Bearer authentication with settings from configuration
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "AspireApp";
        var jwtAudience = configuration["Jwt:Audience"] ?? "AspireApp";

        services.AddAuthentication(options =>
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

        return services;
    }

    /// <summary>
    /// Configures role-based authorization policies
    /// </summary>
    public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"))
            .AddPolicy("UserOrAbove", policy => policy.RequireRole("User", "Manager", "Admin"));

        return services;
    }

    /// <summary>
    /// Configures permission-based authorization with custom policy provider and handler
    /// </summary>
    public static IServiceCollection AddPermissionBasedAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, Authorization.PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, Authorization.PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Configures complete authentication and authorization setup (JWT + Roles + Permissions)
    /// </summary>
    public static IServiceCollection AddAuthenticationAndAuthorization(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration);
        services.AddRoleBasedAuthorization();
        services.AddPermissionBasedAuthorization();

        return services;
    }
}

