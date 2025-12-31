using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.FileUpload.DTOs;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.FileUploads.Interfaces;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.FileUpload.UseCases;

public class GetFileUploadUseCase : BaseUseCase
{
    private readonly IFileUploadRepository _fileUploadRepository;

    public GetFileUploadUseCase(
        IFileUploadRepository fileUploadRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _fileUploadRepository = fileUploadRepository;
    }

    public async Task<Result<FileUploadDto>> ExecuteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileUpload = await _fileUploadRepository.GetAsync(fileId, cancellationToken: cancellationToken);
            if (fileUpload == null)
            {
                return Result.Failure<FileUploadDto>(DomainErrors.General.NotFound("File not found."));
            }

            var fileDto = Mapper.Map<FileUploadDto>(fileUpload);
            return Result.Success(fileDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<FileUploadDto>(DomainErrors.General.InternalError($"Failed to retrieve file: {ex.Message}"));
        }
    }
}

