namespace AspireApp.Modules.FileUpload.Domain.Enums;

/// <summary>
/// Represents the storage type for file uploads
/// </summary>
public enum FileStorageType
{
    /// <summary>
    /// Store file in the server's file system
    /// </summary>
    FileSystem = 1,

    /// <summary>
    /// Store file in the database as binary data
    /// </summary>
    Database = 2,

    /// <summary>
    /// Store file in Cloudflare R2 (S3-compatible storage)
    /// 
    /// WARNING: R2 storage implementation is not fully tested or complete.
    /// Use with caution and ensure thorough testing before production deployment.
    /// </summary>
    R2 = 3
}

