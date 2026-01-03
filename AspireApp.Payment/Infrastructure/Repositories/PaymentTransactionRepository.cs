using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.ApiService.Infrastructure.Repositories;
using AspireApp.Payment.Domain.Entities;
using AspireApp.Payment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Payment.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for PaymentTransaction entity
/// </summary>
public class PaymentTransactionRepository : Repository<PaymentTransaction>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PaymentTransaction>> GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.PaymentId == paymentId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }
}

