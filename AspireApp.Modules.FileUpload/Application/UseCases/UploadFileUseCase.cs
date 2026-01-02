using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Application.DTOs;
using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.Modules.FileUpload.Application.UseCases;

public class UploadFileUseCase : BaseUseCase
{
    private readonly IFileUploadRepository _fileUploadRepository;
    private readonly IFileStorageStrategyFactory _storageStrategyFactory;
    private readonly IFileUploadManager _fileUploadManager;
    private readonly IBackgroundTaskQueue? _backgroundTaskQueue;
    private readonly IServiceScopeFactory? _serviceScopeFactory;

    public UploadFileUseCase(
        IFileUploadRepository fileUploadRepository,
        IFileStorageStrategyFactory storageStrategyFactory,
        IFileUploadManager fileUploadManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        IBackgroundTaskQueue? backgroundTaskQueue = null,
        IServiceScopeFactory? serviceScopeFactory = null)
        : base(unitOfWork, mapper)
    {
        _fileUploadRepository = fileUploadRepository;
        _storageStrategyFactory = storageStrategyFactory;
        _fileUploadManager = fileUploadManager;
        _backgroundTaskQueue = backgroundTaskQueue;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<FileUploadDto>> ExecuteAsync(
        string fileName,
        string contentType,
        Stream fileStream,
        long fileSize,
        UploadFileRequest request,
        Guid? uploadedBy = null,
        CancellationToken cancellationToken = default)
    {
        // If background queue is requested and available, process asynchronously
        if (request.UseBackgroundQueue && _backgroundTaskQueue != null && _serviceScopeFactory != null)
        {
            return await ExecuteWithBackgroundQueueAsync(
                fileName,
                contentType,
                fileStream,
                fileSize,
                request,
                uploadedBy,
                cancellationToken);
        }

        // Otherwise, process synchronously
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                // Validate and process file using domain service
                var (extension, fileType, fileHash) = await _fileUploadManager.ValidateAndProcessFileAsync(
                    fileName, contentType, fileSize, fileStream, ct);

                // Get storage strategy
                var storageStrategy = _storageStrategyFactory.GetStrategy(request.StorageType);

                // Upload file using the selected strategy
                string storagePath;
                byte[]? fileContent = null;

                if (request.StorageType == FileStorageType.Database)
                {
                    // For database storage, read the file content into memory
                    fileContent = await _fileUploadManager.ReadFileIntoMemoryAsync(fileStream, fileSize, ct);
                    fileStream.Position = 0;

                    // Get storage identifier (GUID) from strategy
                    storagePath = await storageStrategy.UploadAsync(fileName, contentType, fileStream, ct);
                }
                else
                {
                    // For FileSystem and R2, use the strategy to upload
                    fileStream.Position = 0;
                    storagePath = await storageStrategy.UploadAsync(fileName, contentType, fileStream, ct);
                }

                // Create FileUpload entity using domain service
                var fileUpload = _fileUploadManager.CreateFileUpload(
                    fileName: fileName,
                    contentType: contentType,
                    fileSize: fileSize,
                    extension: extension,
                    fileType: fileType,
                    storageType: request.StorageType,
                    uploadedBy: uploadedBy,
                    description: request.Description,
                    tags: request.Tags,
                    hash: fileHash,
                    storagePath: storagePath,
                    fileContent: fileContent);

                // Save to database
                var savedFile = await _fileUploadRepository.InsertAsync(fileUpload, ct);

                // Map to DTO
                var fileDto = Mapper.Map<FileUploadDto>(savedFile);

                return Result.Success(fileDto);
            }
            catch (NotSupportedException ex)
            {
                return Result.Failure<FileUploadDto>(DomainErrors.General.InvalidInput(ex.Message));
            }
            catch (DomainException ex)
            {
                return Result.Failure<FileUploadDto>(ex.Error);
            }
            catch (Exception ex)
            {
                return Result.Failure<FileUploadDto>(DomainErrors.General.InternalError($"Failed to upload file: {ex.Message}"));
            }
        }, cancellationToken);
    }

    private async Task<Result<FileUploadDto>> ExecuteWithBackgroundQueueAsync(
        string fileName,
        string contentType,
        Stream fileStream,
        long fileSize,
        UploadFileRequest request,
        Guid? uploadedBy,
        CancellationToken cancellationToken)
    {
        try
        {
            // Read file into memory first (required since HTTP stream will be disposed)
            var fileBytes = await _fileUploadManager.ReadFileIntoMemoryAsync(fileStream, fileSize, cancellationToken);

            // Validate and process file using domain service
            using var validationStream = new MemoryStream(fileBytes);
            var (extension, fileType, fileHash) = await _fileUploadManager.ValidateAndProcessFileAsync(
                fileName, contentType, fileSize, validationStream, cancellationToken);

            // Create FileUpload entity with null StoragePath (will be updated in background)
            var fileUpload = _fileUploadManager.CreateFileUpload(
                fileName: fileName,
                contentType: contentType,
                fileSize: fileSize,
                extension: extension,
                fileType: fileType,
                storageType: request.StorageType,
                uploadedBy: uploadedBy,
                description: request.Description,
                tags: request.Tags,
                hash: fileHash,
                storagePath: null, // Will be set in background processing
                fileContent: request.StorageType == FileStorageType.Database ? fileBytes : null);

            // Save to database first to get the ID
            var savedFile = await ExecuteAndSaveAsync(async ct =>
            {
                var inserted = await _fileUploadRepository.InsertAsync(fileUpload, ct);
                return Result.Success(inserted);
            }, cancellationToken);

            if (!savedFile.IsSuccess)
            {
                return Result.Failure<FileUploadDto>(savedFile.Error);
            }

            var fileId = savedFile.Value.Id;

            // Queue background processing
            _backgroundTaskQueue!.QueueBackgroundWorkItem(async token =>
            {
                try
                {
                    // Create a new scope for background processing
                    using var scope = _serviceScopeFactory!.CreateScope();
                    var backgroundRepository = scope.ServiceProvider.GetRequiredService<IFileUploadRepository>();
                    var backgroundStorageFactory = scope.ServiceProvider.GetRequiredService<IFileStorageStrategyFactory>();
                    var backgroundUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    // Get the file entity
                    var fileEntity = await backgroundRepository.GetAsync(fileId, cancellationToken: token);
                    if (fileEntity == null)
                    {
                        // File not found, skip processing
                        return;
                    }

                    // Get storage strategy
                    var storageStrategy = backgroundStorageFactory.GetStrategy(request.StorageType);

                    // Upload file using the selected strategy
                    string storagePath;
                    using var fileStreamForUpload = new MemoryStream(fileBytes);

                    if (request.StorageType == FileStorageType.Database)
                    {
                        // For database storage, the content is already in FileContent
                        // Just get the storage identifier
                        storagePath = await storageStrategy.UploadAsync(fileName, contentType, fileStreamForUpload, token);
                    }
                    else
                    {
                        // For FileSystem and R2, upload the file
                        storagePath = await storageStrategy.UploadAsync(fileName, contentType, fileStreamForUpload, token);
                    }

                    // Update the file entity with storage path
                    fileEntity.StoragePath = storagePath;

                    // Save changes
                    await backgroundRepository.UpdateAsync(fileEntity, token);
                    await backgroundUnitOfWork.SaveChangesAsync(token);
                }
                catch (Exception ex)
                {
                    // Log error but don't throw - background task should handle errors gracefully
                    // In a production scenario, you might want to update the file status or log to a separate error tracking system
                    System.Diagnostics.Debug.WriteLine($"Background file upload failed for file {fileId}: {ex.Message}");
                }
            });

            // Map to DTO and return immediately
            var fileDto = Mapper.Map<FileUploadDto>(savedFile.Value);
            return Result.Success(fileDto);
        }
        catch (NotSupportedException ex)
        {
            return Result.Failure<FileUploadDto>(DomainErrors.General.InvalidInput(ex.Message));
        }
        catch (DomainException ex)
        {
            return Result.Failure<FileUploadDto>(ex.Error);
        }
        catch (Exception ex)
        {
            return Result.Failure<FileUploadDto>(DomainErrors.General.InternalError($"Failed to queue file upload: {ex.Message}"));
        }
    }
}

