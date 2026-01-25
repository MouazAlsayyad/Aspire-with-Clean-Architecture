using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Application.UseCases;
using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Models;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AspireApp.PaymentTests.Application.UseCases;

public class CreatePaymentUseCaseTests
{
    private readonly Mock<IPaymentStrategyFactory> _strategyFactoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IPaymentManager> _paymentManagerMock;
    private readonly Mock<IValidator<CreatePaymentDto>> _validatorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPaymentStrategy> _paymentStrategyMock;

    private readonly CreatePaymentUseCase _useCase;

    public CreatePaymentUseCaseTests()
    {
        _strategyFactoryMock = new Mock<IPaymentStrategyFactory>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _paymentManagerMock = new Mock<IPaymentManager>();
        _validatorMock = new Mock<IValidator<CreatePaymentDto>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _paymentStrategyMock = new Mock<IPaymentStrategy>();

        _useCase = new CreatePaymentUseCase(
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
        var dto = new CreatePaymentDto();
        var validationFailure = new ValidationFailure("Property", "Error message");
        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("General.InvalidInput");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenPaymentCreatedSuccessfully()
    {
        // Arrange
        var dto = new CreatePaymentDto
        {
            Amount = 100,
            Currency = "USD",
            Method = PaymentMethod.Stripe
        };

        var orderNumber = "ORD-123";
        var paymentResult = new PaymentResult
        {
            Success = true,
            Status = PaymentStatus.Succeeded,
            ExternalReference = "ext-ref",
            PaymentUrl = "http://example.com"
        };

        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _paymentManagerMock.Setup(m => m.GenerateOrderNumber())
            .Returns(orderNumber);

        _strategyFactoryMock.Setup(f => f.GetStrategy(dto.Method))
            .Returns(_paymentStrategyMock.Object);

        _mapperMock.Setup(m => m.Map<CreatePaymentRequest>(dto))
            .Returns(new CreatePaymentRequest());

        _paymentStrategyMock.Setup(s => s.CreatePaymentAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        _mapperMock.Setup(m => m.Map<PaymentDto>(It.IsAny<Payment.Domain.Entities.Payment>()))
            .Returns(new PaymentDto());

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Success.Should().BeTrue();
        result.Value.Status.Should().Be(PaymentStatus.Succeeded);

        _paymentRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<Payment.Domain.Entities.Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment.Domain.Entities.Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
