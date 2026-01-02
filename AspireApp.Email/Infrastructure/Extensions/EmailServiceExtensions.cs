using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Domain.Services;
using AspireApp.Email.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Email.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering email services
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Registers the appropriate email service based on configuration
    /// </summary>
    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Email:Provider"] ?? "SMTP";

        switch (provider.ToUpperInvariant())
        {
            case "SENDGRID":
                services.AddScoped<IEmailService, SendGridEmailService>();
                break;
            case "SMTP":
            default:
                services.AddScoped<IEmailService, SmtpEmailService>();
                break;
        }

        // Register email template provider
        services.AddScoped<IEmailTemplateProvider, EmailTemplateProvider>();
        
        // Register email domain manager
        services.AddScoped<IEmailManager, EmailManager>();

        return services;
    }
}

