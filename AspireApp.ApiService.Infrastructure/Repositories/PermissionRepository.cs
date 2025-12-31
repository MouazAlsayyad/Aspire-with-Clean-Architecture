using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Repositories;

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        // Global query filter automatically excludes deleted entities
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default)
    {
        // Global query filter automatically excludes deleted entities
        return await _context.Permissions
            .Where(p => p.Resource == resource)
            .ToListAsync(cancellationToken);
    }
}

