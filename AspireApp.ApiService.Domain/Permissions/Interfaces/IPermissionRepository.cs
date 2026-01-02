using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Entities;

namespace AspireApp.ApiService.Domain.Permissions.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default);
}

