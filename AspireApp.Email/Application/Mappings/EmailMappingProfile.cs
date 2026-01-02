using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Domain.Entities;
using AutoMapper;

namespace AspireApp.Email.Application.Mappings;

/// <summary>
/// AutoMapper profile for Email module
/// </summary>
public class EmailMappingProfile : Profile
{
    public EmailMappingProfile()
    {
        CreateMap<EmailLog, EmailLogDto>();
    }
}

