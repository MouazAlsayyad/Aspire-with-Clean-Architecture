using AspireApp.Email.Domain.Interfaces;
using AspireApp.Email.Domain.Options;
using AspireApp.Email.Domain.Services;
using AspireApp.Email.Infrastructure.Services;
using AspireApp.Email.Infrastructure.Templates.Strategies;
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
        // Configure email options
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        
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

        // Register email template strategies
        services.AddScoped<IBookingEmailTemplateStrategy, BookingEmailTemplateStrategy>();
        services.AddScoped<ICompletedBookingEmailTemplateStrategy, CompletedBookingEmailTemplateStrategy>();
        services.AddScoped<IMembershipEmailTemplateStrategy, MembershipEmailTemplateStrategy>();
        services.AddScoped<IPayoutConfirmationEmailTemplateStrategy, PayoutConfirmationEmailTemplateStrategy>();
        services.AddScoped<IPayoutRejectionEmailTemplateStrategy, PayoutRejectionEmailTemplateStrategy>();
        services.AddScoped<ISubscriptionEmailTemplateStrategy, SubscriptionEmailTemplateStrategy>();
        
        // Register email template provider
        services.AddScoped<IEmailTemplateProvider, EmailTemplateProvider>();
        
        // Register email domain manager
        services.AddScoped<IEmailManager, EmailManager>();

        return services;
    }
}

