using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Domain.Entities;
using AutoMapper;

namespace AspireApp.Twilio.Application.Mappings;

public class TwilioMappingProfile : Profile
{
    public TwilioMappingProfile()
    {
        CreateMap<Message, MessageDto>();
    }
}

