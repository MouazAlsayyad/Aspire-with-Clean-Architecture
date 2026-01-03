using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Domain.Entities;
using AspireApp.Payment.Domain.Models;
using AutoMapper;
using Newtonsoft.Json;

namespace AspireApp.Payment.Application.Mappings;

/// <summary>
/// AutoMapper profile for payment mappings
/// </summary>
public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        // Entity to DTO mappings
        CreateMap<Domain.Entities.Payment, PaymentDto>();
        CreateMap<PaymentTransaction, PaymentTransactionDto>();

        // DTO to Domain model mappings
        CreateMap<CreatePaymentDto, CreatePaymentRequest>()
            .ForMember(dest => dest.OrderNumber, opt => opt.Ignore()) // Will be generated
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                src.Metadata != null ? JsonConvert.SerializeObject(src.Metadata) : null));

        CreateMap<ProcessPaymentDto, ProcessPaymentRequest>();
        
        CreateMap<RefundPaymentDto, RefundPaymentRequest>()
            .ForMember(dest => dest.ExternalReference, opt => opt.Ignore()); // Will be set from payment entity
    }
}

