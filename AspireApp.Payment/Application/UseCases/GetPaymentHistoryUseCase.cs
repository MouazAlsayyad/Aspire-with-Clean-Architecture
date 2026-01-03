using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Domain.Interfaces;
using AutoMapper;

namespace AspireApp.Payment.Application.UseCases;

/// <summary>
/// Use case for getting payment transaction history
/// </summary>
public class GetPaymentHistoryUseCase : BaseUseCase
{
    private readonly IPaymentTransactionRepository _transactionRepository;

    public GetPaymentHistoryUseCase(
        IPaymentTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<IEnumerable<PaymentTransactionDto>>> ExecuteAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetByPaymentIdAsync(paymentId, cancellationToken);
        
        var transactionDtos = Mapper.Map<IEnumerable<PaymentTransactionDto>>(transactions);
        return Result<IEnumerable<PaymentTransactionDto>>.Success(transactionDtos);
    }
}

