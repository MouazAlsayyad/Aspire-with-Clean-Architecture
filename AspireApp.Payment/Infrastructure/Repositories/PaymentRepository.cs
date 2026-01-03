using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Payment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Payment.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Payment entity
/// </summary>
public class PaymentRepository : Repository<Domain.Entities.Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Payment?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<Domain.Entities.Payment?> GetByExternalReferenceAsync(
        string externalReference,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.ExternalReference == externalReference, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Payment>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Transactions)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreationTime)
            .ToListAsync(cancellationToken);
    }
}

