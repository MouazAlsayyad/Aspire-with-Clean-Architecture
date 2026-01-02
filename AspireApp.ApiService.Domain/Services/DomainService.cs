using AspireApp.Domain.Shared.Interfaces;

namespace AspireApp.ApiService.Domain.Services;

/// <summary>
/// Base class for domain services (managers).
/// Domain services contain business logic that doesn't naturally fit within a single entity.
/// </summary>
public abstract class DomainService : IDomainService
{
    //  this base class typically includes:
    // - Logger
    // - CurrentUser service
    // - Localization
    // - Authorization helpers
    // For now, we'll keep it simple and extend as needed
}
