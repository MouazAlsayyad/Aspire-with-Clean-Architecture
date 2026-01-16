using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Application.UseCases;
using AspireApp.Twilio.Domain.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.Twilio.Tests.Application.UseCases;

public class ValidateOtpUseCaseTests
{
    private readonly ITwilioSmsManager _twilioSmsManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ValidateOtpUseCase _sut;

    public ValidateOtpUseCaseTests()
    {
        _twilioSmsManager = Substitute.For<ITwilioSmsManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();

        _sut = new ValidateOtpUseCase(
            _twilioSmsManager,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessTrue_WhenOtpIsValid()
    {
        // Arrange
        var validateOtpDto = new ValidateOtpDto(
            PhoneNumber: "+1234567890",
            OtpCode: "123456"
        );

        _twilioSmsManager.ValidateOtpAsync(
            validateOtpDto.PhoneNumber,
            validateOtpDto.OtpCode,
            Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));


        // Act
        var result = await _sut.ExecuteAsync(validateOtpDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        await _twilioSmsManager.Received(1).ValidateOtpAsync(
            validateOtpDto.PhoneNumber,
            validateOtpDto.OtpCode,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessFalse_WhenOtpIsInvalid()
    {
        // Arrange
        var validateOtpDto = new ValidateOtpDto(
            PhoneNumber: "+1234567890",
            OtpCode: "654321"
        );

        _twilioSmsManager.ValidateOtpAsync(
            validateOtpDto.PhoneNumber,
            validateOtpDto.OtpCode,
            Arg.Any<CancellationToken>())
            .Returns(false);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        // Act
        var result = await _sut.ExecuteAsync(validateOtpDto);

        // Assert
        Assert.True(result.IsSuccess); // Result itself is successful, but value is false
        Assert.False(result.Value);
        await _twilioSmsManager.Received(1).ValidateOtpAsync(
            validateOtpDto.PhoneNumber,
            validateOtpDto.OtpCode,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var validateOtpDto = new ValidateOtpDto(
            PhoneNumber: "+1234567890",
            OtpCode: "123456"
        );

        _twilioSmsManager.ValidateOtpAsync(
            validateOtpDto.PhoneNumber,
            validateOtpDto.OtpCode,
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Validation error")));

        // Act
        var result = await _sut.ExecuteAsync(validateOtpDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Validation error", result.Error.Message);
        await _twilioSmsManager.Received(1).ValidateOtpAsync(
            validateOtpDto.PhoneNumber,
            validateOtpDto.OtpCode,
            Arg.Any<CancellationToken>());
    }
}