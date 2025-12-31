using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.FileUpload;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.FileUpload;

public class GetAllFileUploadsUseCase : BaseUseCase
{
    private readonly IFileUploadRepository _fileUploadRepository;

    public GetAllFileUploadsUseCase(
        IFileUploadRepository fileUploadRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _fileUploadRepository = fileUploadRepository;
    }

    public async Task<Result<List<FileUploadDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var fileUploads = await _fileUploadRepository.GetListAsync(cancellationToken: cancellationToken);
            var fileDtos = Mapper.Map<List<FileUploadDto>>(fileUploads);
            return Result.Success(fileDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<FileUploadDto>>(
                Domain.Common.DomainErrors.General.InternalError($"Failed to retrieve files: {ex.Message}"));
        }
    }
}

