using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Domain.Entities;
using AspireApp.Payment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AspireApp.Payment.Infrastructure.Repositories;

public class CachedPaymentRepository : CachedRepository<Domain.Entities.Payment>, IPaymentRepository
{
    private readonly IPaymentRepository _paymentRepository;

    public CachedPaymentRepository(
        IPaymentRepository decorated,
        ICacheService cacheService,
        ILogger<CachedPaymentRepository> logger)
        : base(decorated, cacheService, logger)
    {
        _paymentRepository = decorated;
    }

    public async Task<Domain.Entities.Payment?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        string key = $"payment:ordernumber:{orderNumber}";
        
        return await _cacheService.GetOrSetAsync(
            key,
            ct => _paymentRepository.GetByOrderNumberAsync(orderNumber, ct),
            TimeSpanConstants.ThirtyMinutes,
            cancellationToken);
    }

    public async Task<Domain.Entities.Payment?> GetByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default)
    {
        string key = $"payment:extref:{externalReference}";
        
        return await _cacheService.GetOrSetAsync(
            key,
            ct => _paymentRepository.GetByExternalReferenceAsync(externalReference, ct),
            TimeSpanConstants.ThirtyMinutes,
            cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string key = $"payment:user:{userId}";
        
        var result = await _cacheService.GetOrSetAsync(
            key,
            ct => _paymentRepository.GetByUserIdAsync(userId, ct),
            TimeSpanConstants.TenMinutes,
            cancellationToken);

        return result ?? Enumerable.Empty<Domain.Entities.Payment>();
    }
}
