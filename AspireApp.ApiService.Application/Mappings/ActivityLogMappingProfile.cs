using AspireApp.ApiService.Application.DTOs.ActivityLog;
using AspireApp.ApiService.Domain.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Application.Mappings;

public class ActivityLogMappingProfile : Profile
{
    public ActivityLogMappingProfile()
    {
        CreateMap<ActivityLog, ActivityLogDto>();
    }
}

