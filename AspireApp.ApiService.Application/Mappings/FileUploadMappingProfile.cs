using AspireApp.ApiService.Application.DTOs.FileUpload;
using AspireApp.ApiService.Domain.FileUploads.Entities;
using AutoMapper;

namespace AspireApp.ApiService.Application.Mappings;

public class FileUploadMappingProfile : Profile
{
    public FileUploadMappingProfile()
    {
        CreateMap<FileUpload, FileUploadDto>();
    }
}

