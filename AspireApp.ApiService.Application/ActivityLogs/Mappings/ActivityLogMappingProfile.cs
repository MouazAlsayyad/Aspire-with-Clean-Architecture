using AspireApp.ApiService.Application.ActivityLogs.DTOs;
using AspireApp.ApiService.Domain.ActivityLogs.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Application.ActivityLogs.Mappings;

public class ActivityLogMappingProfile : Profile
{
    public ActivityLogMappingProfile()
    {
        CreateMap<ActivityLog, ActivityLogDto>();
    }
}

