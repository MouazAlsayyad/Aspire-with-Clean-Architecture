using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Domain.Entities;
using AspireApp.Payment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.Payment.Infrastructure.Repositories;

public class CachedPaymentTransactionRepository : CachedRepository<PaymentTransaction>, IPaymentTransactionRepository
{
    private readonly IPaymentTransactionRepository _transactionRepository;

    public CachedPaymentTransactionRepository(
        IPaymentTransactionRepository decorated,
        ICacheService cacheService,
        ILogger<CachedPaymentTransactionRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _transactionRepository = decorated;
    }

    public async Task<IEnumerable<PaymentTransaction>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        string key = $"payment_transaction:paymentid:{paymentId}";
        
        var result = await _cacheService.GetOrSetAsync(
            key,
            ct => _transactionRepository.GetByPaymentIdAsync(paymentId, ct),
            TimeSpanConstants.ThirtyMinutes,
            cancellationToken);

        return result ?? Enumerable.Empty<PaymentTransaction>();
    }
}
