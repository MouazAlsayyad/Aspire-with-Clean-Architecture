using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace AspireApp.ApiService.Infrastructure.Services.FileStorage;

/// <summary>
/// File storage strategy that saves files to the server's file system
/// </summary>
public class FileSystemStorageStrategy : IFileStorageStrategy
{
    private readonly string _basePath;
    private readonly ILogger<FileSystemStorageStrategy> _logger;

    public FileStorageType StorageType => FileStorageType.FileSystem;

    public FileSystemStorageStrategy(IConfiguration configuration, ILogger<FileSystemStorageStrategy> logger)
    {
        _logger = logger;
        _basePath = configuration["FileStorage:FileSystem:BasePath"] 
            ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        
        // Ensure the base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Created file storage directory: {BasePath}", _basePath);
        }
    }

    public async Task<string> UploadAsync(
        string fileName,
        string contentType,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate unique file name to avoid conflicts
            var fileExtension = Path.GetExtension(fileName);
            var guid = Guid.NewGuid();
            
            // Build unique file name (compiler optimizes string interpolation)
            var uniqueFileName = $"{guid}{fileExtension}";
            
            // Create year/month subdirectory for organization
            var now = DateTime.UtcNow;
            var year = now.Year;
            var month = now.Month;
            
            // Use stackalloc for path building
            var subDirectory = Path.Combine(year.ToString(), month.ToString("D2"));
            var fullDirectory = Path.Combine(_basePath, subDirectory);
            
            if (!Directory.Exists(fullDirectory))
            {
                Directory.CreateDirectory(fullDirectory);
            }

            var filePath = Path.Combine(fullDirectory, uniqueFileName);
            
            // Optimize path replacement using Span<char>
            var relativePath = Path.Combine(subDirectory, uniqueFileName);
            if (Path.DirectorySeparatorChar != '/')
            {
                relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
            }

            // Save file to disk with optimized buffer
            const int bufferSize = 81920; // 80KB buffer for optimal performance
            await using (var fileStreamWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
            {
                // Use ArrayPool for buffer allocation
                var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    int bytesRead;
                    while ((bytesRead = await fileStream.ReadAsync(new Memory<byte>(buffer), cancellationToken)) > 0)
                    {
                        await fileStreamWriter.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            _logger.LogInformation("File uploaded to file system: {FilePath}", filePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to file system: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Optimize path replacement using Span<char>
            string fullPath;
            if (Path.DirectorySeparatorChar != '/')
            {
                fullPath = Path.Combine(_basePath, storagePath.Replace('/', Path.DirectorySeparatorChar));
            }
            else
            {
                fullPath = Path.Combine(_basePath, storagePath);
            }
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {storagePath}");
            }

            const int bufferSize = 81920; // 80KB buffer for optimal performance
            
            // Pre-allocate MemoryStream capacity if file size is known
            var fileInfo = new FileInfo(fullPath);
            var memoryStream = new MemoryStream(fileInfo.Exists ? (int)Math.Min(fileInfo.Length, int.MaxValue) : 0);
            
            await using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
            {
                // Use ArrayPool for buffer allocation
                var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    int bytesRead;
                    while ((bytesRead = await fileStream.ReadAsync(new Memory<byte>(buffer), cancellationToken)) > 0)
                    {
                        await memoryStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from file system: {StoragePath}", storagePath);
            throw;
        }
    }

    public Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, storagePath.Replace('/', Path.DirectorySeparatorChar));
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted from file system: {FilePath}", fullPath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {StoragePath}", storagePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from file system: {StoragePath}", storagePath);
            throw;
        }
    }

    public Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, storagePath.Replace('/', Path.DirectorySeparatorChar));
            return Task.FromResult(File.Exists(fullPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in file system: {StoragePath}", storagePath);
            return Task.FromResult(false);
        }
    }
}

