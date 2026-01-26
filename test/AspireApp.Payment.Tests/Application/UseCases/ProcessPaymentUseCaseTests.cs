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

public class ProcessPaymentUseCaseTests
{
    private readonly Mock<IPaymentStrategyFactory> _strategyFactoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IValidator<ProcessPaymentDto>> _validatorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPaymentStrategy> _paymentStrategyMock;
    private readonly ProcessPaymentUseCase _useCase;

    public ProcessPaymentUseCaseTests()
    {
        _strategyFactoryMock = new Mock<IPaymentStrategyFactory>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _validatorMock = new Mock<IValidator<ProcessPaymentDto>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _paymentStrategyMock = new Mock<IPaymentStrategy>();

        _useCase = new ProcessPaymentUseCase(
            _strategyFactoryMock.Object,
            _paymentRepositoryMock.Object,
            _validatorMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var dto = new ProcessPaymentDto();
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
        var dto = new ProcessPaymentDto { PaymentId = Guid.NewGuid() };
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
    public async Task ExecuteAsync_ShouldProcessPaymentSuccessfully()
    {
        // Arrange
        var dto = new ProcessPaymentDto { PaymentId = Guid.NewGuid(), ExternalReference = "ext-ref" };
        var payment = new AspireApp.Payment.Domain.Entities.Payment("ORD-123", PaymentMethod.Stripe, new Money(100, Currency.Usd));

        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _paymentRepositoryMock.Setup(r => r.GetAsync(dto.PaymentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _strategyFactoryMock.Setup(f => f.GetStrategy(PaymentMethod.Stripe))
            .Returns(_paymentStrategyMock.Object);

        _mapperMock.Setup(m => m.Map<ProcessPaymentRequest>(dto))
            .Returns(new ProcessPaymentRequest());

        var paymentResult = new PaymentResult
        {
            Success = true,
            Status = PaymentStatus.Succeeded,
            ExternalReference = "new-ref"
        };

        _paymentStrategyMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        _mapperMock.Setup(m => m.Map<PaymentDto>(payment))
            .Returns(new PaymentDto());

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.ExternalReference.Should().Be("new-ref");
        _paymentRepositoryMock.Verify(r => r.UpdateAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }
}
