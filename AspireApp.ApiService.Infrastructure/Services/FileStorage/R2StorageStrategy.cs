using Amazon.S3;
using Amazon.S3.Model;
using AspireApp.ApiService.Domain.FileUploads.Enums;
using AspireApp.ApiService.Domain.FileUploads.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AspireApp.ApiService.Infrastructure.Services.FileStorage;

/// <summary>
/// File storage strategy for Cloudflare R2 (S3-compatible storage).
/// 
/// WARNING: This implementation is not fully tested or complete.
/// It has been implemented with basic functionality (upload, download, delete, exists)
/// but requires thorough testing, error handling improvements, and edge case coverage
/// before being used in production environments.
/// 
/// Known limitations:
/// - Limited error handling and retry logic
/// - No support for presigned URLs or public access configuration
/// - Memory stream usage in DownloadAsync may cause issues with large files
/// - No connection pooling or performance optimizations
/// - Missing comprehensive integration tests
/// </summary>
public class R2StorageStrategy : IFileStorageStrategy, IAsyncDisposable, IDisposable
{
    private readonly ILogger<R2StorageStrategy> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string? _basePath;

    public FileStorageType StorageType => FileStorageType.R2;

    public R2StorageStrategy(
        IConfiguration configuration,
        ILogger<R2StorageStrategy> logger)
    {
        _logger = logger;

        // Get R2 configuration
        var accountId = configuration["FileStorage:R2:AccountId"];
        var accessKeyId = configuration["FileStorage:R2:AccessKeyId"];
        var secretAccessKey = configuration["FileStorage:R2:SecretAccessKey"];
        _bucketName = configuration["FileStorage:R2:BucketName"] ?? "";
        _basePath = configuration["FileStorage:R2:BasePath"];

        if (string.IsNullOrWhiteSpace(accountId) || accountId == "your-account-id")
        {
            throw new InvalidOperationException("Cloudflare R2 AccountId is not configured correctly. Please check your appsettings.json.");
        }

        if (string.IsNullOrWhiteSpace(accessKeyId) || accessKeyId == "your-access-key")
        {
            throw new InvalidOperationException("Cloudflare R2 AccessKeyId is not configured correctly.");
        }

        if (string.IsNullOrWhiteSpace(secretAccessKey) || secretAccessKey == "your-secret-access-key")
        {
            throw new InvalidOperationException("Cloudflare R2 SecretAccessKey is not configured correctly.");
        }

        if (string.IsNullOrWhiteSpace(_bucketName) || _bucketName == "your-bucket-name")
        {
            throw new InvalidOperationException("Cloudflare R2 BucketName is not configured correctly.");
        }

        // Configure S3 client for Cloudflare R2
        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true // R2 requires path-style addressing
        };

        _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);
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
            var uniqueFileName = $"{guid}{fileExtension}";

            // Create year/month subdirectory for organization
            var now = DateTime.UtcNow;
            var year = now.Year;
            var month = now.Month;
            var subDirectory = $"{year}/{month:D2}";

            // Build the object key (path)
            var objectKey = string.IsNullOrWhiteSpace(_basePath)
                ? $"{subDirectory}/{uniqueFileName}"
                : $"{_basePath.TrimEnd('/')}/{subDirectory}/{uniqueFileName}";

            // Create upload request
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.None // R2 doesn't support SSE
            };

            // Upload file
            await _s3Client.PutObjectAsync(putRequest, cancellationToken);

            _logger.LogInformation("File uploaded to R2: {ObjectKey}", objectKey);
            return objectKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to R2: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = storagePath
            };

            var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);

            // Copy the response stream to a MemoryStream so we can return it
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File not found in R2: {StoragePath}", storagePath);
            throw new FileNotFoundException($"File not found: {storagePath}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from R2: {StoragePath}", storagePath);
            throw;
        }
    }

    public async Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = storagePath
            };

            await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            _logger.LogInformation("File deleted from R2: {StoragePath}", storagePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from R2: {StoragePath}", storagePath);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = storagePath
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in R2: {StoragePath}", storagePath);
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _s3Client.Dispose();
        await ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        _s3Client.Dispose();
    }
}

