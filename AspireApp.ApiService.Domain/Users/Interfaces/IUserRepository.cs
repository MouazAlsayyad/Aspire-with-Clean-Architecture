using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Entities;

namespace AspireApp.ApiService.Domain.Users.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
}

