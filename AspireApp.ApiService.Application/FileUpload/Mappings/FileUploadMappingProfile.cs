using AspireApp.ApiService.Application.FileUpload.DTOs;
using AutoMapper;

namespace AspireApp.ApiService.Application.FileUpload.Mappings;

public class FileUploadMappingProfile : Profile
{
    public FileUploadMappingProfile()
    {
        CreateMap<Domain.FileUploads.Entities.FileUpload, FileUploadDto>();
    }
}

