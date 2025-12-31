using AspireApp.ApiService.Notifications.Application.DTOs;
using AspireApp.ApiService.Notifications.Domain.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Notifications.Application.Mappings;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationDto>()
            .ConstructUsing(n => new NotificationDto(
                n.Id,
                n.Type,
                n.Priority,
                n.Status,
                n.IsRead,
                n.ReadAt,
                n.Title,
                n.TitleAr,
                n.Message,
                n.MessageAr,
                n.ActionUrl,
                n.UserId,
                n.CreationTime
            ));
    }
}

