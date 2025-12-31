using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Domain.Users.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Application.Users.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ConstructUsing(u => new UserDto(
                u.Id,
                u.Email,
                u.UserName,
                u.FirstName,
                u.LastName,
                u.IsEmailConfirmed,
                u.IsActive,
                u.Language,
                GetRoleNames(u.UserRoles)
            ));
    }

    private static IEnumerable<string> GetRoleNames(IEnumerable<UserRole> userRoles)
    {
        return userRoles
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role!.Name)
            .Where(n => !string.IsNullOrEmpty(n));
    }
}

