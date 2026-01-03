using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Models;
using AutoMapper;
using FluentValidation;

namespace AspireApp.Payment.Application.UseCases;

/// <summary>
/// Use case for creating a payment
/// </summary>
public class CreatePaymentUseCase : BaseUseCase
{
    private readonly IPaymentStrategyFactory _strategyFactory;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentManager _paymentManager;
    private readonly IValidator<CreatePaymentDto> _validator;

    public CreatePaymentUseCase(
        IPaymentStrategyFactory strategyFactory,
        IPaymentRepository paymentRepository,
        IPaymentManager paymentManager,
        IValidator<CreatePaymentDto> validator,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _strategyFactory = strategyFactory;
        _paymentRepository = paymentRepository;
        _paymentManager = paymentManager;
        _validator = validator;
    }

    public async Task<Result<PaymentResultDto>> ExecuteAsync(
        CreatePaymentDto dto,
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

            // Generate order number
            var orderNumber = _paymentManager.GenerateOrderNumber();

            // Validate payment request
            _paymentManager.ValidatePaymentRequest(dto.Amount, dto.Currency, orderNumber);

            // Create payment entity
            var payment = new Domain.Entities.Payment(
                orderNumber,
                dto.Method,
                dto.Amount,
                dto.Currency,
                dto.UserId,
                dto.CustomerEmail,
                dto.CustomerPhone,
                dto.Metadata != null ? Newtonsoft.Json.JsonConvert.SerializeObject(dto.Metadata) : null);

            // Save payment entity
            await _paymentRepository.InsertAsync(payment, ct);

            // Get payment strategy
            var strategy = _strategyFactory.GetStrategy(dto.Method);

            // Map DTO to request
            var request = Mapper.Map<CreatePaymentRequest>(dto);
            request.OrderNumber = orderNumber;

            // Create payment with provider
            var paymentResult = await strategy.CreatePaymentAsync(request, ct);

            // Update payment status and external reference
            payment.UpdateStatus(paymentResult.Status, paymentResult.ExternalReference);
            payment.AddTransaction(
                Domain.Enums.TransactionType.Authorization,
                dto.Amount,
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
                PaymentUrl = paymentResult.PaymentUrl,
                Payment = Mapper.Map<PaymentDto>(payment)
            };

            return Result.Success(resultDto);
        }, cancellationToken);
    }
}

