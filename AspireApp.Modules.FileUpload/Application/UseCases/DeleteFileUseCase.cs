using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;

namespace AspireApp.Modules.FileUpload.Application.UseCases;

public class DeleteFileUseCase : BaseUseCase
{
    private readonly IFileUploadRepository _fileUploadRepository;
    private readonly IFileStorageStrategyFactory _storageStrategyFactory;

    public DeleteFileUseCase(
        IFileUploadRepository fileUploadRepository,
        IFileStorageStrategyFactory storageStrategyFactory,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _fileUploadRepository = fileUploadRepository;
        _storageStrategyFactory = storageStrategyFactory;
    }

    public async Task<Result> ExecuteAsync(Guid fileId, Guid? deletedBy = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                var fileUpload = await _fileUploadRepository.GetAsync(fileId, cancellationToken: ct);
                if (fileUpload == null)
                {
                    return Result.Failure(DomainErrors.General.NotFound("File not found."));
                }

                // Delete from storage (except Database, which is handled by entity deletion)
                if (fileUpload.StorageType != FileStorageType.Database && !string.IsNullOrWhiteSpace(fileUpload.StoragePath))
                {
                    try
                    {
                        var storageStrategy = _storageStrategyFactory.GetStrategy(fileUpload.StorageType);
                        await storageStrategy.DeleteAsync(fileUpload.StoragePath, ct);
                    }
                    catch
                    {
                        // Log but don't fail if storage deletion fails (file might already be deleted)
                        // Continue with entity deletion
                    }
                }

                // Soft delete the entity
                await _fileUploadRepository.DeleteAsync(fileUpload, ct);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(DomainErrors.General.InternalError($"Failed to delete file: {ex.Message}"));
            }
        }, cancellationToken);
    }
}

