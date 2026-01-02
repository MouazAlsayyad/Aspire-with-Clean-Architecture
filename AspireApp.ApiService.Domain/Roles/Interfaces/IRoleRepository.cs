using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Domain.Roles.Entities;

namespace AspireApp.ApiService.Domain.Roles.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

