using AspireApp.ApiService.Application.DTOs.Role;
using AspireApp.ApiService.Domain.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Application.Mappings;

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

