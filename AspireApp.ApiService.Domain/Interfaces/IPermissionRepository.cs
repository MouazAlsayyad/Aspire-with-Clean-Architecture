using AspireApp.ApiService.Domain.Entities;

namespace AspireApp.ApiService.Domain.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default);
}

