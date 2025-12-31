using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.FileUpload;
using AspireApp.ApiService.Application.Helpers;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.FileUpload;

public class UploadFileUseCase : BaseUseCase
{
    private readonly IFileUploadRepository _fileUploadRepository;
    private readonly IFileStorageStrategyFactory _storageStrategyFactory;

    public UploadFileUseCase(
        IFileUploadRepository fileUploadRepository,
        IFileStorageStrategyFactory storageStrategyFactory,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _fileUploadRepository = fileUploadRepository;
        _storageStrategyFactory = storageStrategyFactory;
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
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                // Validate file size
                if (fileSize <= 0)
                {
                    return Result.Failure<FileUploadDto>(DomainErrors.General.InvalidInput("File size must be greater than zero."));
                }

                // Get file extension
                var extension = Path.GetExtension(fileName);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    return Result.Failure<FileUploadDto>(DomainErrors.General.InvalidInput("File must have an extension."));
                }

                // Determine file type
                var fileType = FileTypeHelper.GetFileType(extension, contentType);

                // Validate file size for the file type
                if (!FileValidationHelper.IsValidFileSize(fileSize, fileType))
                {
                    var maxSize = FileValidationHelper.GetMaxFileSize(fileType);
                    return Result.Failure<FileUploadDto>(
                        DomainErrors.General.InvalidInput($"File size exceeds maximum allowed size of {maxSize / (1024 * 1024)} MB for {fileType} files."));
                }

                // For images, validate extension and content type
                if (fileType == FileType.Image)
                {
                    if (!FileValidationHelper.IsImageFile(extension, contentType))
                    {
                        return Result.Failure<FileUploadDto>(
                            DomainErrors.General.InvalidInput("Invalid image file. Only JPG, JPEG, PNG, GIF, BMP, WEBP, and SVG are allowed."));
                    }
                }

                // Compute file hash for duplicate detection
                string? fileHash = null;
                try
                {
                    fileHash = await FileValidationHelper.ComputeHashAsync(fileStream);
                }
                catch
                {
                    // Hash computation failure is not critical, continue without hash
                }

                // Get storage strategy
                var storageStrategy = _storageStrategyFactory.GetStrategy(request.StorageType);

                // Upload file using the selected strategy
                string storagePath;
                byte[]? fileContent = null;

                if (request.StorageType == FileStorageType.Database)
                {
                    // For database storage, read the file content into memory using optimized buffer
                    fileStream.Position = 0;
                    const int bufferSize = 81920; // 80KB buffer for optimal performance
                    using var memoryStream = new MemoryStream((int)fileSize); // Pre-allocate capacity
                    
                    // Use ArrayPool for buffer allocation
                    var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);
                    try
                    {
                        int bytesRead;
                        while ((bytesRead = await fileStream.ReadAsync(new Memory<byte>(buffer), ct)) > 0)
                        {
                            await memoryStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), ct);
                        }
                    }
                    finally
                    {
                        System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
                    }
                    
                    fileContent = memoryStream.ToArray();
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

                // Create FileUpload entity
                var fileUpload = new Domain.Entities.FileUpload(
                    fileName: fileName,
                    contentType: contentType,
                    fileSize: fileSize,
                    extension: extension,
                    fileType: fileType,
                    storageType: request.StorageType,
                    uploadedBy: uploadedBy,
                    description: request.Description,
                    tags: request.Tags,
                    hash: fileHash);

                fileUpload.StoragePath = storagePath;
                fileUpload.FileContent = fileContent; // Only populated for Database storage

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
}

