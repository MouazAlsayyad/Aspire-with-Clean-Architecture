using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Infrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AspireApp.PaymentTests.Infrastructure.Factories;

public class PaymentStrategyFactoryTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly PaymentStrategyFactory _factory;

    public PaymentStrategyFactoryTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _factory = new PaymentStrategyFactory(_serviceProviderMock.Object);
    }

    [Fact]
    public void GetStrategy_ShouldReturnCashStrategy_WhenMethodIsCash()
    {
        // Arrange
        var strategyMock = new Mock<ICashPaymentStrategy>();
        _serviceProviderMock.Setup(x => x.GetService(typeof(ICashPaymentStrategy)))
            .Returns(strategyMock.Object);

        // Act
        var strategy = _factory.GetStrategy(PaymentMethod.Cash);

        // Assert
        strategy.Should().Be(strategyMock.Object);
    }

    [Fact]
    public void GetStrategy_ShouldReturnStripeStrategy_WhenMethodIsStripe()
    {
        // Arrange
        var strategyMock = new Mock<IStripePaymentStrategy>();
        _serviceProviderMock.Setup(x => x.GetService(typeof(IStripePaymentStrategy)))
            .Returns(strategyMock.Object);

        // Act
        var strategy = _factory.GetStrategy(PaymentMethod.Stripe);

        // Assert
        strategy.Should().Be(strategyMock.Object);
    }

    [Fact]
    public void GetStrategy_ShouldReturnTabbyStrategy_WhenMethodIsTabby()
    {
        // Arrange
        var strategyMock = new Mock<ITabbyPaymentStrategy>();
        _serviceProviderMock.Setup(x => x.GetService(typeof(ITabbyPaymentStrategy)))
            .Returns(strategyMock.Object);

        // Act
        var strategy = _factory.GetStrategy(PaymentMethod.Tabby);

        // Assert
        strategy.Should().Be(strategyMock.Object);
    }

    [Fact]
    public void GetStrategy_ShouldThrowException_WhenMethodIsUnknown()
    {
        // Arrange
        var unknownMethod = (PaymentMethod)999;

        // Act
        var act = () => _factory.GetStrategy(unknownMethod);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Unknown payment method: {unknownMethod} (Parameter 'method')");
    }
}
