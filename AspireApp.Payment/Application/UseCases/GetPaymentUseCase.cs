using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Domain.Interfaces;
using AutoMapper;

namespace AspireApp.Payment.Application.UseCases;

/// <summary>
/// Use case for getting a payment by ID
/// </summary>
public class GetPaymentUseCase : BaseUseCase
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentUseCase(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<PaymentDto>> ExecuteAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetAsync(paymentId, false, cancellationToken);
        
        if (payment == null)
        {
            return DomainErrors.General.NotFound($"Payment with ID {paymentId} not found");
        }

        var paymentDto = Mapper.Map<PaymentDto>(payment);
        return Result<PaymentDto>.Success(paymentDto);
    }
}

