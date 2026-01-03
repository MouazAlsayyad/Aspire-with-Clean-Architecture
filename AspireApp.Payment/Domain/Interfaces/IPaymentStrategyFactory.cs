using AspireApp.Payment.Domain.Enums;

namespace AspireApp.Payment.Domain.Interfaces;

/// <summary>
/// Factory for creating payment strategies
/// </summary>
public interface IPaymentStrategyFactory
{
    /// <summary>
    /// Gets the appropriate strategy for the specified payment method
    /// </summary>
    IPaymentStrategy GetStrategy(PaymentMethod method);
}

