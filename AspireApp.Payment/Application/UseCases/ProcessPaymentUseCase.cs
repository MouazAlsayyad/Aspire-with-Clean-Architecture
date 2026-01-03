using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Models;
using AutoMapper;
using FluentValidation;

namespace AspireApp.Payment.Application.UseCases;

/// <summary>
/// Use case for processing a payment
/// </summary>
public class ProcessPaymentUseCase : BaseUseCase
{
    private readonly IPaymentStrategyFactory _strategyFactory;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IValidator<ProcessPaymentDto> _validator;

    public ProcessPaymentUseCase(
        IPaymentStrategyFactory strategyFactory,
        IPaymentRepository paymentRepository,
        IValidator<ProcessPaymentDto> validator,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _strategyFactory = strategyFactory;
        _paymentRepository = paymentRepository;
        _validator = validator;
    }

    public async Task<Result<PaymentResultDto>> ExecuteAsync(
        ProcessPaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync<PaymentResultDto>(async ct =>
        {
            // Validate input
            var validationResult = await _validator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
            {
                return Result.Failure<PaymentResultDto>(
                    DomainErrors.General.InvalidInput(validationResult.Errors.First().ErrorMessage));
            }

            // Get payment
            var payment = await _paymentRepository.GetAsync(dto.PaymentId, false, ct);
            if (payment == null)
            {
                return Result.Failure<PaymentResultDto>(
                    DomainErrors.General.NotFound($"Payment with ID {dto.PaymentId} not found"));
            }

            // Get payment strategy
            var strategy = _strategyFactory.GetStrategy(payment.Method);

            // Process payment
            var request = Mapper.Map<ProcessPaymentRequest>(dto);
            request.ExternalReference = dto.ExternalReference ?? payment.ExternalReference;
            
            var paymentResult = await strategy.ProcessPaymentAsync(request, ct);

            // Update payment status
            payment.UpdateStatus(paymentResult.Status, paymentResult.ExternalReference);
            payment.AddTransaction(
                Domain.Enums.TransactionType.Capture,
                payment.Amount,
                paymentResult.Status,
                paymentResult.ErrorMessage);

            await _paymentRepository.UpdateAsync(payment, ct);

            // Map to result DTO
            var resultDto = new PaymentResultDto
            {
                Success = paymentResult.Success,
                ErrorMessage = paymentResult.ErrorMessage,
                Status = paymentResult.Status,
                ExternalReference = paymentResult.ExternalReference,
                Payment = Mapper.Map<PaymentDto>(payment)
            };

            return Result.Success(resultDto);
        }, cancellationToken);
    }
}

