using AspireApp.Modules.ActivityLogs.Application.DTOs;
using AspireApp.Modules.ActivityLogs.Domain.Entities;
using AutoMapper;

namespace AspireApp.Modules.ActivityLogs.Application.Mappings;

public class ActivityLogMappingProfile : Profile
{
    public ActivityLogMappingProfile()
    {
        CreateMap<ActivityLog, ActivityLogDto>();
    }
}

