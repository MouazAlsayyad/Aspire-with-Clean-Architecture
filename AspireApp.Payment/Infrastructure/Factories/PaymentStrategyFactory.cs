using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Payment.Infrastructure.Factories;

/// <summary>
/// Factory for creating payment strategies based on payment method
/// </summary>
public class PaymentStrategyFactory : IPaymentStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentStrategy GetStrategy(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => _serviceProvider.GetRequiredService<ICashPaymentStrategy>(),
            PaymentMethod.Stripe => _serviceProvider.GetRequiredService<IStripePaymentStrategy>(),
            PaymentMethod.Tabby => _serviceProvider.GetRequiredService<ITabbyPaymentStrategy>(),
            _ => throw new ArgumentException($"Unknown payment method: {method}", nameof(method))
        };
    }
}

