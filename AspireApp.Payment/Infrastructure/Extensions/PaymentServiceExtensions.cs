using AspireApp.Payment.Application.Validators;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Options;
using AspireApp.Payment.Domain.Services;
using AspireApp.Payment.Infrastructure.Factories;
using AspireApp.Payment.Infrastructure.Repositories;
using AspireApp.Payment.Infrastructure.Services;
using AspireApp.Payment.Infrastructure.Strategies;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Payment.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering payment services
/// </summary>
public static class PaymentServiceExtensions
{
    /// <summary>
    /// Registers all payment services and strategies
    /// </summary>
    public static IServiceCollection AddPaymentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<TabbyOptions>(configuration.GetSection(TabbyOptions.SectionName));

        // Register strategies
        services.AddScoped<ICashPaymentStrategy, CashPaymentStrategy>();
        services.AddScoped<IStripePaymentStrategy, StripePaymentStrategy>();
        services.AddScoped<ITabbyPaymentStrategy, TabbyPaymentStrategy>();

        // Register factory
        services.AddScoped<IPaymentStrategyFactory, PaymentStrategyFactory>();

        // Register repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

        // Register domain manager
        services.AddScoped<IPaymentManager, PaymentManager>();

        // Register external services
        services.AddScoped<StripePaymentService>();
        services.AddHttpClient<TabbyPaymentService>();

        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreatePaymentDtoValidator>();

        // Note: Payment event handlers are automatically registered by AddDomainEventHandlers()
        // to send notifications when payments succeed/fail/refund

        return services;
    }
}

