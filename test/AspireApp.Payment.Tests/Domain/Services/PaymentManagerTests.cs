using AspireApp.Domain.Shared.Common;
using AspireApp.Payment.Domain.Entities;
using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Services;
using AspireApp.Payment.Domain.ValueObjects;
using FluentAssertions;

namespace AspireApp.Payment.Tests.Domain.Services;

public class PaymentManagerTests
{
    private readonly PaymentManager _paymentManager;

    public PaymentManagerTests()
    {
        _paymentManager = new PaymentManager();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000000)]
    public void ValidatePaymentRequest_ShouldThrowException_WhenAmountIsInvalid(decimal amount)
    {
        // Act
        var act = () => _paymentManager.ValidatePaymentRequest(amount, "USD", "ORD-123");

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Error.Code == DomainErrors.General.InvalidInput(string.Empty).Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void ValidatePaymentRequest_ShouldThrowException_WhenCurrencyIsInvalid(string? currency)
    {
        // Act
        var act = () => _paymentManager.ValidatePaymentRequest(100, currency!, "ORD-123");

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Error.Code == DomainErrors.General.InvalidInput(string.Empty).Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidatePaymentRequest_ShouldThrowException_WhenOrderNumberIsInvalid(string? orderNumber)
    {
        // Act
        var act = () => _paymentManager.ValidatePaymentRequest(100, "USD", orderNumber!);

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Error.Code == DomainErrors.General.InvalidInput(string.Empty).Code);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    public void ValidatePaymentRequest_ShouldNotThrowException_WhenInputIsValid(string currency)
    {
        // Act
        var act = () => _paymentManager.ValidatePaymentRequest(100, currency, "ORD-123");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateOrderNumber_ShouldReturnValidOrderNumber()
    {
        // Act
        var orderNumber = _paymentManager.GenerateOrderNumber();

        // Assert
        orderNumber.Should().StartWith("PAY-");
        orderNumber.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void ValidateRefundRequest_ShouldThrowException_WhenStatusCannotBeRefunded()
    {
        // Arrange
        var payment = CreatePayment(PaymentStatus.Pending);

        // Act
        var act = () => _paymentManager.ValidateRefundRequest(payment, 10);

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Error.Code == DomainErrors.General.InvalidInput(string.Empty).Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateRefundRequest_ShouldThrowException_WhenRefundAmountIsInvalid(decimal refundAmount)
    {
        // Arrange
        var payment = CreatePayment(PaymentStatus.Succeeded);

        // Act
        var act = () => _paymentManager.ValidateRefundRequest(payment, refundAmount);

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Error.Code == DomainErrors.General.InvalidInput(string.Empty).Code);
    }

    [Fact]
    public void ValidateRefundRequest_ShouldThrowException_WhenRefundAmountExceedsPaymentAmount()
    {
        // Arrange
        var payment = CreatePayment(PaymentStatus.Succeeded, 100);

        // Act
        var act = () => _paymentManager.ValidateRefundRequest(payment, 101);

        // Assert
        act.Should().Throw<DomainException>()
            .Where(e => e.Error.Code == DomainErrors.General.InvalidInput(string.Empty).Code);
    }

    [Fact]
    public void ValidateRefundRequest_ShouldNotThrowException_WhenInputIsValid()
    {
        // Arrange
        var payment = CreatePayment(PaymentStatus.Succeeded, 100);

        // Act
        var act = () => _paymentManager.ValidateRefundRequest(payment, 50);

        // Assert
        act.Should().NotThrow();
    }

    private AspireApp.Payment.Domain.Entities.Payment CreatePayment(PaymentStatus status, decimal amount = 100)
    {
        var payment = new AspireApp.Payment.Domain.Entities.Payment(
            "ORD-123",
            PaymentMethod.Stripe,
            new Money(amount, Currency.Usd));
        
        if (status != PaymentStatus.Pending)
        {
            payment.UpdateStatus(status);
        }

        return payment;
    }
}
