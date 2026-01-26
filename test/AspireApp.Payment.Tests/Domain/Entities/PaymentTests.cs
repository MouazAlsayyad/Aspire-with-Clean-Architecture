using AspireApp.Payment.Domain.Entities;
using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Events;
using AspireApp.Payment.Domain.ValueObjects;
using FluentAssertions;

namespace AspireApp.Payment.Tests.Domain.Entities;

public class PaymentTests
{
    [Fact]
    public void Constructor_ShouldInitializePaymentCorrectly()
    {
        // Arrange
        var orderNumber = "ORD-123";
        var method = PaymentMethod.Stripe;
        var amount = new Money(100, Currency.Usd);
        var userId = Guid.NewGuid();
        var customerEmail = "test@example.com";
        var customerPhone = "1234567890";
        var metadata = "{\"key\":\"value\"}";

        // Act
        var payment = new AspireApp.Payment.Domain.Entities.Payment(
            orderNumber,
            method,
            amount,
            userId,
            customerEmail,
            customerPhone,
            metadata);

        // Assert
        payment.Id.Should().NotBeEmpty();
        payment.OrderNumber.Should().Be(orderNumber);
        payment.Method.Should().Be(method);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Should().Be(amount);
        payment.UserId.Should().Be(userId);
        payment.CustomerEmail.Should().Be(customerEmail);
        payment.CustomerPhone.Should().Be(customerPhone);
        payment.Metadata.Should().Be(metadata);
        payment.Transactions.Should().BeEmpty();
        payment.DomainEvents.Should().ContainSingle(e => e is PaymentCreatedEvent);
    }

    [Theory]
    [InlineData(PaymentStatus.Processing, typeof(PaymentProcessingEvent))]
    [InlineData(PaymentStatus.Authorized, typeof(PaymentAuthorizedEvent))]
    [InlineData(PaymentStatus.Succeeded, typeof(PaymentSucceededEvent))]
    [InlineData(PaymentStatus.Failed, typeof(PaymentFailedEvent))]
    [InlineData(PaymentStatus.Refunded, typeof(PaymentRefundedEvent))]
    [InlineData(PaymentStatus.PartiallyRefunded, typeof(PaymentRefundedEvent))]
    public void UpdateStatus_ShouldUpdateStatusAndRaiseEvent(PaymentStatus newStatus, Type expectedEventType)
    {
        // Arrange
        var payment = CreatePayment();
        var externalReference = "ext-ref";

        // Act
        payment.UpdateStatus(newStatus, externalReference);

        // Assert
        payment.Status.Should().Be(newStatus);
        payment.ExternalReference.Should().Be(externalReference);
        payment.DomainEvents.Should().Contain(e => e.GetType() == expectedEventType);
    }

    [Fact]
    public void AddTransaction_ShouldAddTransactionToCollection()
    {
        // Arrange
        var payment = CreatePayment();
        var type = TransactionType.Authorization;
        var amount = new Money(100, Currency.Usd);
        var status = PaymentStatus.Authorized;
        var response = "success";

        // Act
        payment.AddTransaction(type, amount, status, response);

        // Assert
        payment.Transactions.Should().HaveCount(1);
        var transaction = payment.Transactions.First();
        transaction.PaymentId.Should().Be(payment.Id);
        transaction.Type.Should().Be(type);
        transaction.Amount.Should().Be(amount);
        transaction.Status.Should().Be(status);
        transaction.Response.Should().Be(response);
    }

    private AspireApp.Payment.Domain.Entities.Payment CreatePayment()
    {
        return new AspireApp.Payment.Domain.Entities.Payment(
            "ORD-123",
            PaymentMethod.Stripe,
            new Money(100, Currency.Usd));
    }
}
