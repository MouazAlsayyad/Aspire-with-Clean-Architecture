using AspireApp.ApiService.Domain.Entities;

namespace AspireApp.ApiService.Domain.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

