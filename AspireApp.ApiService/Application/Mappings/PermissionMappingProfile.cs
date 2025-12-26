using AspireApp.ApiService.Application.DTOs.Permission;
using AspireApp.ApiService.Domain.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Application.Mappings;

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

