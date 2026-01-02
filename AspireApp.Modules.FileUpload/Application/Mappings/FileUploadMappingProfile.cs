using AspireApp.Modules.FileUpload.Application.DTOs;
using AspireApp.Modules.FileUpload.Domain.Entities;
using AutoMapper;
using FileUploadEntity = AspireApp.Modules.FileUpload.Domain.Entities.FileUpload;

namespace AspireApp.Modules.FileUpload.Application.Mappings;

public class FileUploadMappingProfile : Profile
{
    public FileUploadMappingProfile()
    {
        CreateMap<FileUploadEntity, FileUploadDto>();
    }
}

