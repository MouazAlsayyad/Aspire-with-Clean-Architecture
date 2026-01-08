using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.Domain.Shared.Tests.Common;

public class BaseUseCaseTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly TestUseCase _useCase;

    public BaseUseCaseTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _useCase = new TestUseCase(_unitOfWork, _mapper);
    }

    [Fact]
    public async Task ExecuteAndSaveAsync_WithSuccessResult_ShouldSaveChanges()
    {
        // Arrange
        var operation = (CancellationToken ct) => Task.FromResult(Result.Success("Test"));

        // Act
        var result = await _useCase.TestExecuteAndSaveAsyncGeneric(operation);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test", result.Value);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAndSaveAsync_WithFailureResult_ShouldNotSaveChanges()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Test error message");
        var operation = (CancellationToken ct) => Task.FromResult(Result.Failure<string>(error));

        // Act
        var result = await _useCase.TestExecuteAndSaveAsyncGeneric(operation);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAndSaveAsync_NonGeneric_WithSuccessResult_ShouldSaveChanges()
    {
        // Arrange
        var operation = (CancellationToken ct) => Task.FromResult(Result.Success());

        // Act
        var result = await _useCase.TestExecuteAndSaveAsync(operation);

        // Assert
        Assert.True(result.IsSuccess);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAndSaveAsync_NonGeneric_WithFailureResult_ShouldNotSaveChanges()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Test error message");
        var operation = (CancellationToken ct) => Task.FromResult(Result.Failure(error));

        // Act
        var result = await _useCase.TestExecuteAndSaveAsync(operation);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAndSaveAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var operation = (CancellationToken ct) => Task.FromResult(Result.Success("Test"));

        // Act
        await _useCase.TestExecuteAndSaveAsyncGeneric(operation, cts.Token);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(cts.Token);
    }

    // Test implementation class to expose protected methods
    private class TestUseCase : BaseUseCase
    {
        public TestUseCase(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }

        public Task<Result<T>> TestExecuteAndSaveAsyncGeneric<T>(
            Func<CancellationToken, Task<Result<T>>> operation,
            CancellationToken cancellationToken = default)
        {
            return ExecuteAndSaveAsync(operation, cancellationToken);
        }

        public Task<Result> TestExecuteAndSaveAsync(
            Func<CancellationToken, Task<Result>> operation,
            CancellationToken cancellationToken = default)
        {
            return ExecuteAndSaveAsync(operation, cancellationToken);
        }
    }
}

