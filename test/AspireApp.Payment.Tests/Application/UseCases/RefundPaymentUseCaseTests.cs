using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Application.UseCases;
using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Models;
using AspireApp.Payment.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AspireApp.Payment.Tests.Application.UseCases;

public class RefundPaymentUseCaseTests
{
    private readonly Mock<IPaymentStrategyFactory> _strategyFactoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IPaymentManager> _paymentManagerMock;
    private readonly Mock<IValidator<RefundPaymentDto>> _validatorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPaymentStrategy> _paymentStrategyMock;
    private readonly RefundPaymentUseCase _useCase;

    public RefundPaymentUseCaseTests()
    {
        _strategyFactoryMock = new Mock<IPaymentStrategyFactory>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _paymentManagerMock = new Mock<IPaymentManager>();
        _validatorMock = new Mock<IValidator<RefundPaymentDto>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _paymentStrategyMock = new Mock<IPaymentStrategy>();

        _useCase = new RefundPaymentUseCase(
            _strategyFactoryMock.Object,
            _paymentRepositoryMock.Object,
            _paymentManagerMock.Object,
            _validatorMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var dto = new RefundPaymentDto();
        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Prop", "Error") }));

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var dto = new RefundPaymentDto { PaymentId = Guid.NewGuid() };
        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _paymentRepositoryMock.Setup(r => r.GetAsync(dto.PaymentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AspireApp.Payment.Domain.Entities.Payment?)null);

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("General.NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenNoExternalReference()
    {
        // Arrange
        var dto = new RefundPaymentDto { PaymentId = Guid.NewGuid(), Amount = 100 };
        var payment = new AspireApp.Payment.Domain.Entities.Payment("ORD-123", PaymentMethod.Stripe, new Money(100, Currency.Usd));
        // Payment initialized with no external reference by default if not set

        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _paymentRepositoryMock.Setup(r => r.GetAsync(dto.PaymentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _paymentManagerMock.Setup(m => m.ValidateRefundRequest(payment, dto.Amount)); // Passes

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("external reference");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRefundSuccessfully()
    {
        // Arrange
        var dto = new RefundPaymentDto { PaymentId = Guid.NewGuid(), Amount = 100 };
        var payment = new AspireApp.Payment.Domain.Entities.Payment("ORD-123", PaymentMethod.Stripe, new Money(100, Currency.Usd));
        payment.UpdateStatus(PaymentStatus.Succeeded, "ext-ref"); // Set external reference

        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _paymentRepositoryMock.Setup(r => r.GetAsync(dto.PaymentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _strategyFactoryMock.Setup(f => f.GetStrategy(PaymentMethod.Stripe))
            .Returns(_paymentStrategyMock.Object);

        var refundResult = new RefundResult { Success = true, RefundId = "ref-123" };
        _paymentStrategyMock.Setup(s => s.RefundPaymentAsync(It.IsAny<RefundPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refundResult);

        _mapperMock.Setup(m => m.Map<PaymentDto>(payment))
            .Returns(new PaymentDto());

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Refunded);
        _paymentRepositoryMock.Verify(r => r.UpdateAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }
}
