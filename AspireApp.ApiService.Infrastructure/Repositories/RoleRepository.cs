using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Role?> GetAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsQueryable();

        // If including deleted, ignore the global query filter
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public override async Task<List<Role>> GetListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsQueryable();

        // If including deleted, ignore the global query filter
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.ToListAsync(cancellationToken);
    }


    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        // Global query filter automatically excludes deleted entities
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }
}

