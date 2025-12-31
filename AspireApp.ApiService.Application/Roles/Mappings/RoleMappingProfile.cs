using AspireApp.ApiService.Application.Roles.DTOs;
using AspireApp.ApiService.Domain.Roles.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Application.Roles.Mappings;

public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleDto>()
            .ConstructUsing(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.Type.ToString(),
                GetPermissionNames(r.RolePermissions)
            ));
    }

    private static IEnumerable<string> GetPermissionNames(IEnumerable<RolePermission> rolePermissions)
    {
        return rolePermissions
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .Where(n => !string.IsNullOrEmpty(n));
    }
}

