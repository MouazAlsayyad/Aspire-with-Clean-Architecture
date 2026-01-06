using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Domain.Enums;
using AspireApp.Payment.Domain.Interfaces;
using AspireApp.Payment.Domain.Models;
using AspireApp.Payment.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;

namespace AspireApp.Payment.Application.UseCases;

/// <summary>
/// Use case for refunding a payment
/// </summary>
public class RefundPaymentUseCase : BaseUseCase
{
    private readonly IPaymentStrategyFactory _strategyFactory;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentManager _paymentManager;
    private readonly IValidator<RefundPaymentDto> _validator;

    public RefundPaymentUseCase(
        IPaymentStrategyFactory strategyFactory,
        IPaymentRepository paymentRepository,
        IPaymentManager paymentManager,
        IValidator<RefundPaymentDto> validator,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _strategyFactory = strategyFactory;
        _paymentRepository = paymentRepository;
        _paymentManager = paymentManager;
        _validator = validator;
    }

    public async Task<Result<PaymentDto>> ExecuteAsync(
        RefundPaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync<PaymentDto>(async ct =>
        {
            // Validate input
            var validationResult = await _validator.ValidateAsync(dto, ct);
            if (!validationResult.IsValid)
            {
                return Result.Failure<PaymentDto>(
                    DomainErrors.General.InvalidInput(validationResult.Errors.First().ErrorMessage));
            }

            // Get payment
            var payment = await _paymentRepository.GetAsync(dto.PaymentId, false, ct);
            if (payment == null)
            {
                return Result.Failure<PaymentDto>(
                    DomainErrors.General.NotFound($"Payment with ID {dto.PaymentId} not found"));
            }

            // Validate refund
            _paymentManager.ValidateRefundRequest(payment, dto.Amount);

            if (string.IsNullOrEmpty(payment.ExternalReference))
            {
                return Result.Failure<PaymentDto>(
                    DomainErrors.General.InvalidInput("Payment has no external reference for refund"));
            }

            // Get payment strategy
            var strategy = _strategyFactory.GetStrategy(payment.Method);

            // Process refund
            var request = new RefundPaymentRequest
            {
                PaymentId = dto.PaymentId,
                ExternalReference = payment.ExternalReference,
                Amount = dto.Amount,
                Reason = dto.Reason
            };

            var refundResult = await strategy.RefundPaymentAsync(request, ct);

            if (!refundResult.Success)
            {
                return Result.Failure<PaymentDto>(
                    DomainErrors.General.InternalError(refundResult.ErrorMessage ?? "Refund failed"));
            }

            // Update payment status
            var refundMoney = new Money(dto.Amount, payment.Amount.Currency);
            var isPartialRefund = dto.Amount < payment.Amount.Amount;
            var newStatus = isPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;

            payment.UpdateStatus(newStatus);
            payment.AddTransaction(
                TransactionType.Refund,
                refundMoney,
                newStatus,
                $"Refund ID: {refundResult.RefundId}");

            await _paymentRepository.UpdateAsync(payment, ct);

            var paymentDto = Mapper.Map<PaymentDto>(payment);
            return Result.Success(paymentDto);
        }, cancellationToken);
    }
}

