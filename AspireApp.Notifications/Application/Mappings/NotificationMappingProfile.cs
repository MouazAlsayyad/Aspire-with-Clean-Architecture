using AspireApp.Notifications.Application.DTOs;
using AspireApp.Notifications.Domain.Models;
using AutoMapper;

namespace AspireApp.Notifications.Application.Mappings;

/// <summary>
/// AutoMapper profile for notification mappings
/// </summary>
public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<SendNotificationDto, NotificationRequest>();
        CreateMap<NotificationResult, NotificationResultDto>();
    }
}

