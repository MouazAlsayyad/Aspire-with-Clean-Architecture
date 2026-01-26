using AspireApp.Domain.Shared.Common;
using AspireApp.Payment.Domain.ValueObjects;
using FluentAssertions;

namespace AspireApp.Payment.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        var amount = 100m;
        var currency = Currency.Usd;

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void OperatorPlus_ShouldAddAmounts_WhenCurrenciesMatch()
    {
        // Arrange
        var moneyTwice = new Money(100, Currency.Usd);
        var moneyOnce = new Money(50, Currency.Usd);

        // Act
        var result = moneyTwice + moneyOnce;

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be(Currency.Usd);
    }

    [Fact]
    public void OperatorPlus_ShouldThrowException_WhenCurrenciesMismatch()
    {
        // Arrange
        var moneyUsd = new Money(100, Currency.Usd);
        var moneyEur = new Money(50, Currency.Eur);

        // Act
        var act = () => { var result = moneyUsd + moneyEur; };

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Error.Code == DomainErrors.General.InvalidInput(string.Empty).Code);
    }

    [Fact]
    public void OperatorMinus_ShouldSubtractAmounts_WhenCurrenciesMatch()
    {
        // Arrange
        var moneyTwice = new Money(100, Currency.Usd);
        var moneyOnce = new Money(50, Currency.Usd);

        // Act
        var result = moneyTwice - moneyOnce;

        // Assert
        result.Amount.Should().Be(50);
        result.Currency.Should().Be(Currency.Usd);
    }

    [Fact]
    public void OperatorMinus_ShouldThrowException_WhenCurrenciesMismatch()
    {
        // Arrange
        var moneyUsd = new Money(100, Currency.Usd);
        var moneyEur = new Money(50, Currency.Eur);

        // Act
        var act = () => { var result = moneyUsd - moneyEur; };

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Error.Code == DomainErrors.General.InvalidInput(string.Empty).Code);
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        // Act
        var zero = Money.Zero(Currency.Usd);

        // Assert
        zero.Amount.Should().Be(0);
        zero.Currency.Should().Be(Currency.Usd);
    }

    [Fact]
    public void IsZero_ShouldReturnTrue_WhenAmountIsZero()
    {
        // Arrange
        var money = new Money(0, Currency.Usd);

        // Act
        var isZero = money.IsZero();

        // Assert
        isZero.Should().BeTrue();
    }

    [Fact]
    public void IsZero_ShouldReturnFalse_WhenAmountIsNotZero()
    {
        // Arrange
        var money = new Money(10, Currency.Usd);

        // Act
        var isZero = money.IsZero();

        // Assert
        isZero.Should().BeFalse();
    }
}
