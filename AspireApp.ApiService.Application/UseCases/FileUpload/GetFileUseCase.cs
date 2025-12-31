using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.FileUpload;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.FileUpload;

public class GetFileUseCase : BaseUseCase
{
    private readonly IFileUploadRepository _fileUploadRepository;
    private readonly IFileStorageStrategyFactory _storageStrategyFactory;

    public GetFileUseCase(
        IFileUploadRepository fileUploadRepository,
        IFileStorageStrategyFactory storageStrategyFactory,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _fileUploadRepository = fileUploadRepository;
        _storageStrategyFactory = storageStrategyFactory;
    }

    public async Task<Result<FileDownloadDto>> ExecuteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileUpload = await _fileUploadRepository.GetAsync(fileId, cancellationToken: cancellationToken);
            if (fileUpload == null)
            {
                return Result.Failure<FileDownloadDto>(DomainErrors.General.NotFound("File not found."));
            }

            Stream fileStream;

            if (fileUpload.StorageType == Domain.Enums.FileStorageType.Database)
            {
                // For database storage, return the file content from the entity
                if (fileUpload.FileContent == null || fileUpload.FileContent.Length == 0)
                {
                    return Result.Failure<FileDownloadDto>(DomainErrors.General.NotFound("File content not found."));
                }
                // Use the byte array directly without copying (MemoryStream will wrap it)
                fileStream = new MemoryStream(fileUpload.FileContent, writable: false);
            }
            else
            {
                // For FileSystem and R2, use the storage strategy to download
                if (string.IsNullOrWhiteSpace(fileUpload.StoragePath))
                {
                    return Result.Failure<FileDownloadDto>(DomainErrors.General.InternalError("File storage path is missing."));
                }

                var storageStrategy = _storageStrategyFactory.GetStrategy(fileUpload.StorageType);
                fileStream = await storageStrategy.DownloadAsync(fileUpload.StoragePath, cancellationToken);
            }

            var downloadDto = new FileDownloadDto
            {
                FileStream = fileStream,
                FileName = fileUpload.FileName,
                ContentType = fileUpload.ContentType
            };

            return Result.Success(downloadDto);
        }
        catch (FileNotFoundException)
        {
            return Result.Failure<FileDownloadDto>(DomainErrors.General.NotFound("File not found."));
        }
        catch (NotSupportedException ex)
        {
            return Result.Failure<FileDownloadDto>(DomainErrors.General.InvalidInput(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<FileDownloadDto>(DomainErrors.General.InternalError($"Failed to retrieve file: {ex.Message}"));
        }
    }
}

