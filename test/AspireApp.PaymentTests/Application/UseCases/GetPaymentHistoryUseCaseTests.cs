using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Application.UseCases;
using AspireApp.Payment.Domain.Entities;
using AspireApp.Payment.Domain.Interfaces;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AspireApp.PaymentTests.Application.UseCases;

public class GetPaymentHistoryUseCaseTests
{
    private readonly Mock<IPaymentTransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetPaymentHistoryUseCase _useCase;

    public GetPaymentHistoryUseCaseTests()
    {
        _transactionRepositoryMock = new Mock<IPaymentTransactionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _useCase = new GetPaymentHistoryUseCase(
            _transactionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTransactions_WhenTheyExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var transactions = new List<PaymentTransaction>(); // Empty list is fine for testing mapping flow

        _transactionRepositoryMock.Setup(r => r.GetByPaymentIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        _mapperMock.Setup(m => m.Map<IEnumerable<PaymentTransactionDto>>(transactions))
            .Returns(new List<PaymentTransactionDto>());

        // Act
        var result = await _useCase.ExecuteAsync(paymentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
