using AspireApp.ApiService.Application.Permissions.DTOs;
using AspireApp.ApiService.Domain.Permissions.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Application.Permissions.Mappings;

public class PermissionMappingProfile : Profile
{
    public PermissionMappingProfile()
    {
        CreateMap<Permission, PermissionDto>()
            .ConstructUsing(p => new PermissionDto(
                p.Id,
                p.Name,
                p.Description,
                p.Resource,
                p.Action,
                p.GetFullPermissionName()
            ));
    }
}

