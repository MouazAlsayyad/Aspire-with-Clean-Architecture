using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Permissions;
using AspireApp.ApiService.Domain.Roles;
using AspireApp.ApiService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed permissions - automatically detect and create missing permissions from PermissionNames
        // Also delete permissions that exist in database but not in code
        var allPermissionDefinitions = PermissionNames.GetAllDefinitions();
        var definedPermissionNames = allPermissionDefinitions.Select(d => d.Name).ToHashSet();
        
        // Get all permissions including deleted ones
        var allExistingPermissions = await context.Permissions
            .IgnoreQueryFilters()
            .ToListAsync();

        // Find permissions that need to be created (don't exist in database)
        var missingPermissions = allPermissionDefinitions
            .Where(def => !allExistingPermissions.Any(p => p.Name == def.Name))
            .Select(def => new Permission(def.Name, def.Description, def.Resource, def.Action))
            .ToList();

        // Find permissions that exist in database but not in code (orphaned permissions)
        var orphanedPermissions = allExistingPermissions
            .Where(p => !definedPermissionNames.Contains(p.Name))
            .ToList();

        // Find permissions that are soft-deleted but exist in code (should be restored)
        var permissionsToRestore = allExistingPermissions
            .Where(p => p.IsDeleted && definedPermissionNames.Contains(p.Name))
            .ToList();

        // Create missing permissions
        if (missingPermissions.Any())
        {
            await context.Permissions.AddRangeAsync(missingPermissions);
        }

        // Restore soft-deleted permissions that exist in code
        foreach (var permission in permissionsToRestore)
        {
            permission.Restore();
            context.Permissions.Update(permission);
        }

        // Soft delete orphaned permissions (exist in database but not in code)
        foreach (var permission in orphanedPermissions)
        {
            if (!permission.IsDeleted)
            {
                permission.Delete();
                context.Permissions.Update(permission);
            }
        }

        if (missingPermissions.Any() || permissionsToRestore.Any() || orphanedPermissions.Any(p => !p.IsDeleted))
        {
            await context.SaveChangesAsync();
        }

        // Get all active permissions (after seeding)
        // Refresh from database to ensure we have the latest state including any newly created permissions
        var allPermissions = await context.Permissions
            .ToListAsync();

        // Seed roles if they don't exist
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new Role(RoleNames.Admin, "Administrator with full access", RoleType.Admin);
            var managerRole = new Role(RoleNames.Manager, "Manager with elevated permissions", RoleType.Manager);
            var userRole = new Role(RoleNames.User, "Standard user", RoleType.User);

            // Admin gets all permissions
            foreach (var permission in allPermissions)
            {
                adminRole.AddPermission(permission);
            }

            // Manager gets read permissions
            var readPermissions = allPermissions.Where(p => p.Action == "Read").ToList();
            foreach (var permission in readPermissions)
            {
                managerRole.AddPermission(permission);
            }
            managerRole.AddPermission(allPermissions.First(p => p.Name == PermissionNames.Weather.Write));

            // User gets basic read permissions
            userRole.AddPermission(allPermissions.First(p => p.Name == PermissionNames.Weather.Read));

            await context.Roles.AddRangeAsync(adminRole, managerRole, userRole);
            await context.SaveChangesAsync();
        }
        else
        {
            // Ensure admin role always has all permissions (even if roles already exist)
            // This handles the case when new permissions are added to PermissionNames
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == RoleNames.Admin);

            if (adminRole != null)
            {
                // Get all permission names that should be assigned to admin (all defined permissions)
                var requiredPermissionNames = allPermissionDefinitions.Select(d => d.Name).ToHashSet();

                // Query RolePermissions with Permission included to compare by name
                // This is more reliable than comparing by ID
                var existingAdminPermissionNames = await context.RolePermissions
                    .Where(rp => rp.RoleId == adminRole.Id)
                    .Join(context.Permissions,
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => p.Name)
                    .ToHashSetAsync();

                // Find permissions that admin doesn't have yet by comparing names
                // This ensures all permissions defined in code are assigned to admin
                var missingAdminPermissions = allPermissions
                    .Where(p => requiredPermissionNames.Contains(p.Name) && 
                                !existingAdminPermissionNames.Contains(p.Name))
                    .ToList();

                // Add any missing permissions to admin role using the AddPermission method
                // This ensures proper change tracking and uses the domain method
                if (missingAdminPermissions.Any())
                {
                    // Reload the role with permissions included to use AddPermission method properly
                    var adminRoleWithPermissions = await context.Roles
                        .Include(r => r.RolePermissions)
                        .FirstOrDefaultAsync(r => r.Id == adminRole.Id);

                    if (adminRoleWithPermissions != null)
                    {
                        foreach (var permission in missingAdminPermissions)
                        {
                            adminRoleWithPermissions.AddPermission(permission);
                        }
                        context.Roles.Update(adminRoleWithPermissions);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        // Seed admin user if it doesn't exist
        if (!await context.Users.AnyAsync(u => u.Email == "admin@example.com"))
        {
            // Hash password for admin user (default password: Admin@123)
            var (hash, salt) = passwordHasher.HashPassword("Admin@123");
            var passwordHash = new PasswordHash(hash, salt);

            var adminUser = new User(
                email: "admin@example.com",
                userName: "admin",
                passwordHash: passwordHash,
                firstName: "Admin",
                lastName: "User"
            );

            // Activate and confirm email for admin user
            adminUser.Activate();
            adminUser.ConfirmEmail();

            // Get admin role
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Admin);
            if (adminRole != null)
            {
                adminUser.AddRole(adminRole);
            }

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}

