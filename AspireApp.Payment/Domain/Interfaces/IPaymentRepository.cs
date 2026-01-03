using AspireApp.Domain.Shared.Interfaces;

namespace AspireApp.Payment.Domain.Interfaces;

/// <summary>
/// Repository interface for Payment entity
/// </summary>
public interface IPaymentRepository : IRepository<Entities.Payment>
{
    /// <summary>
    /// Gets a payment by order number
    /// </summary>
    Task<Entities.Payment?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a payment by external reference
    /// </summary>
    Task<Entities.Payment?> GetByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all payments for a user
    /// </summary>
    Task<IEnumerable<Entities.Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

