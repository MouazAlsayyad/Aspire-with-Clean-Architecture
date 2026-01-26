using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Application.UseCases;
using AspireApp.Payment.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AspireApp.Payment.Tests.Application.UseCases;

public class GetPaymentUseCaseTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetPaymentUseCase _useCase;

    public GetPaymentUseCaseTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _useCase = new GetPaymentUseCase(
            _paymentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _paymentRepositoryMock.Setup(r => r.GetAsync(paymentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AspireApp.Payment.Domain.Entities.Payment?)null);

        // Act
        var result = await _useCase.ExecuteAsync(paymentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("General.NotFound");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPayment_WhenPaymentExists()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Mock<AspireApp.Payment.Domain.Entities.Payment>(); // Using Mock or creating instance if possible
                                                                             // Since constructor is private/internal or complex, let's assume we can mock or use a helper if needed. 
                                                                             // Or we can just return null and test the null case, but for success we need an object.
                                                                             // Creating an object via reflection or helper if constructor is restricted. 
                                                                             // Based on previous tests, constructor is accessible or we used a helper method.
                                                                             // Actually, looking at Payment.cs, constructor is public.

        var validPayment = new AspireApp.Payment.Domain.Entities.Payment(
            "ORD-123",
            AspireApp.Payment.Domain.Enums.PaymentMethod.Stripe,
            new AspireApp.Payment.Domain.ValueObjects.Money(100, AspireApp.Payment.Domain.ValueObjects.Currency.Usd));

        _paymentRepositoryMock.Setup(r => r.GetAsync(paymentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validPayment);

        _mapperMock.Setup(m => m.Map<PaymentDto>(validPayment))
            .Returns(new PaymentDto { Id = paymentId });

        // Act
        var result = await _useCase.ExecuteAsync(paymentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(paymentId);
    }
}
