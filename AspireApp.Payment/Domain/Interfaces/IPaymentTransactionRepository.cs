using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Payment.Domain.Entities;

namespace AspireApp.Payment.Domain.Interfaces;

/// <summary>
/// Repository interface for PaymentTransaction entity
/// </summary>
public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
{
    /// <summary>
    /// Gets all transactions for a payment
    /// </summary>
    Task<IEnumerable<PaymentTransaction>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
}

