# Cloudflare R2 Storage Configuration Guide

> **⚠️ WARNING: Cloudflare R2 implementation is not fully tested or complete.**
> 
> This feature has been implemented with basic functionality but requires thorough testing,
> error handling improvements, and edge case coverage before being used in production environments.
> Use with caution and ensure comprehensive testing before deployment.

This guide will walk you through setting up and configuring Cloudflare R2 for file storage in the AspireApp application.

## Prerequisites

- A Cloudflare account
- Access to Cloudflare R2 (available in the Cloudflare dashboard)

## Step 1: Create an R2 Bucket

1. Log in to your [Cloudflare Dashboard](https://dash.cloudflare.com/)
2. Navigate to **R2** in the left sidebar
3. Click **Create bucket**
4. Enter a unique bucket name (e.g., `aspireapp-files`)
5. Choose a location for your bucket (select the region closest to your users)
6. Click **Create bucket**
7. **Note your Bucket Name**: You will need this for the `BucketName` configuration

## Step 2: Get Your Account ID

1. On the R2 overview page (where your buckets are listed), look at the right-hand sidebar
2. Copy the **Account ID** (it's a 32-character hexadecimal string)
3. Alternatively, you can find it by selecting any website/domain in the Cloudflare Dashboard and scrolling down to find your **Account ID** in the right sidebar

## Step 3: Create API Tokens

To access R2 programmatically, you need to create API tokens:

1. In the Cloudflare Dashboard, go to **R2** → **Manage R2 API Tokens**
2. Click **Create API token**
3. Configure the token:
   - **Token name**: Enter a descriptive name (e.g., `AspireApp-R2-Access` or `AspireApp-Storage-Token`)
   - **Permissions**: Select **Edit** or **Object Read & Write** (this allows uploading and deleting files)
   - You can choose to limit this token to specific buckets or allow it for all
   - **TTL**: Set expiration (optional, leave empty for no expiration)
   - **Allowlisted IPs**: (Optional) Restrict access to specific IP addresses
4. Click **Create API Token** or **Create Token**
5. **CRITICAL**: Copy the following values immediately (you won't be able to see them again):
   - **Access Key ID**
   - **Secret Access Key**

## Step 4: Configure the Application

### Option A: Using appsettings.json (Development Only)

**⚠️ Warning**: Never commit sensitive credentials to version control!

Add the following configuration to `appsettings.json` or `appsettings.Development.json` in the `AspireApp.ApiService` project:

```json
{
  "FileStorage": {
    "R2": {
      "AccountId": "YOUR_CLOUDFLARE_ACCOUNT_ID",
      "AccessKeyId": "YOUR_ACCESS_KEY_ID",
      "SecretAccessKey": "YOUR_SECRET_ACCESS_KEY",
      "BucketName": "your-bucket-name",
      "BasePath": "uploads"
    }
  }
}
```

### Option B: Using User Secrets (Recommended for Development)

For local development, use .NET User Secrets to store sensitive credentials:

```bash
cd AspireApp.ApiService
dotnet user-secrets set "FileStorage:R2:AccountId" "your-account-id-here"
dotnet user-secrets set "FileStorage:R2:AccessKeyId" "your-access-key-id-here"
dotnet user-secrets set "FileStorage:R2:SecretAccessKey" "your-secret-access-key-here"
dotnet user-secrets set "FileStorage:R2:BucketName" "your-bucket-name-here"
dotnet user-secrets set "FileStorage:R2:BasePath" "uploads"
```

### Option C: Using Environment Variables (Recommended for Production)

Set the following environment variables:

```bash
FileStorage__R2__AccountId=your-account-id-here
FileStorage__R2__AccessKeyId=your-access-key-id-here
FileStorage__R2__SecretAccessKey=your-secret-access-key-here
FileStorage__R2__BucketName=your-bucket-name-here
FileStorage__R2__BasePath=uploads
```

### Option D: Using Azure Key Vault / AWS Secrets Manager / Other Secret Managers

For production deployments, use a secure secret management service:

1. Store the R2 credentials in your secret management service
2. Configure your application to read from the secret store
3. Map the secrets to the configuration keys above

## Configuration Parameters

| Parameter | Description | Required | Example |
|-----------|-------------|----------|---------|
| `AccountId` | Your Cloudflare Account ID (32-character hex string) | Yes | `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6` |
| `AccessKeyId` | R2 API Token Access Key ID | Yes | `abc123def456...` |
| `SecretAccessKey` | R2 API Token Secret Access Key | Yes | `xyz789uvw012...` |
| `BucketName` | Name of your R2 bucket | Yes | `aspireapp-files` |
| `BasePath` | Optional base path prefix for all uploaded files | No | `uploads` or `production/files` |

### Configuration Fields Explained:

- **AccountId**: Found on the Cloudflare R2 dashboard
- **AccessKeyId**: The "Access Key ID" from your API token
- **SecretAccessKey**: The "Secret Access Key" from your API token
- **BucketName**: The name of the bucket you created (case-sensitive)
- **BasePath** (Optional): A prefix/folder path within the bucket where all files will be stored (e.g., `uploads` or `prod/files`)

## Step 5: Activating R2 in the Application

The application is built with a flexible storage strategy. It supports Local File System, Database, and Cloudflare R2.

### Choosing R2 for Uploads

By default, the application uses `FileSystem` (1). To use R2, you must specify the storage type in your request to the upload endpoint.

**API Endpoint:** `POST /api/files/upload` (or your configured upload route)

**Request Body (Multipart Form-Data):**
- `file`: The actual file to upload
- `StorageType`: `3` (or `R2`)
- `Description`: (Optional) Your file description
- `Tags`: (Optional) Comma-separated tags

The value `3` corresponds to `FileStorageType.R2`.

**Example Request:**
```http
POST https://localhost:7XXX/api/files/upload
Authorization: Bearer <your-token>
Content-Type: multipart/form-data

{
  "file": <file-data>,
  "storageType": 3  // R2 = 3
}
```

## Step 6: Test the Configuration

1. Start your application:
   ```bash
   dotnet run --project AspireApp.AppHost
   ```

2. Upload a file using the API with `StorageType: R2` (see example above)

3. Verify the file appears in your R2 bucket in the Cloudflare Dashboard

## File Organization

Files uploaded to R2 are automatically organized by date:
- Format: `{BasePath}/{Year}/{Month}/{Guid}.{Extension}`
- Example: `uploads/2024/12/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg`

This structure helps with:
- **Organization**: Easy to locate files by date
- **Performance**: Better distribution across R2's infrastructure
- **Uniqueness**: GUID-based filenames prevent conflicts

## Security Best Practices

1. **Never commit credentials**: Use User Secrets, Environment Variables, or Secret Managers
2. **Use least privilege**: Create API tokens with only the permissions needed
3. **Rotate tokens regularly**: Update API tokens periodically
4. **Restrict IP access**: Use allowlisted IPs for API tokens when possible
5. **Enable bucket policies**: Configure bucket-level access policies in Cloudflare
6. **Monitor usage**: Regularly check R2 usage and access logs in Cloudflare Dashboard

## Troubleshooting

### Error: "AccountId is required" or "FileStorage:R2:AccountId is required"
- Ensure `FileStorage:R2:AccountId` is set in your configuration
- Verify your `appsettings.json` is correctly formatted and the keys match the expected structure exactly

### Error: "AccessKeyId is required"
- Verify your API token Access Key ID is correctly configured

### Error: "SecretAccessKey is required"
- Verify your API token Secret Access Key is correctly configured

### Error: "BucketName is required"
- Ensure the bucket name matches exactly (case-sensitive)

### Error: "Access Denied", "Permission Denied (403)", or "403 Forbidden"
- Verify your API token has **Edit** permissions and is authorized for the bucket you are using
- Check that the bucket name is correct
- Ensure your Account ID is correct

### Error: "Bucket not found" or "404 Not Found"
- Verify the bucket exists in your Cloudflare account
- Check the bucket name spelling (case-sensitive)
- Ensure you're using the correct Account ID

### Files not appearing in R2
- Check application logs for upload errors
- Verify the R2 configuration is loaded correctly
- Test with a simple file upload first

### Public Access Issues
By default, R2 buckets are private. If you need to serve files directly via URL, you must either:
- Use the application's download endpoint
- Connect a custom domain to your R2 bucket in the Cloudflare dashboard
- Enable the "R2.dev" subdomain (not recommended for production)

### CORS Errors
If you encounter issues when uploading directly from a web frontend, configure the **CORS Policy** in the Cloudflare bucket settings.

## Cost Considerations

Cloudflare R2 pricing:
- **Storage**: $0.015 per GB/month
- **Class A Operations** (writes): $4.50 per million requests
- **Class B Operations** (reads): $0.36 per million requests
- **Egress**: Free (no egress fees)

For most applications, R2 is significantly cheaper than AWS S3, especially if you have high egress requirements.

## Additional Resources

- [Cloudflare R2 Documentation](https://developers.cloudflare.com/r2/)
- [Cloudflare R2 Pricing](https://developers.cloudflare.com/r2/pricing/)
- [R2 API Tokens Guide](https://developers.cloudflare.com/r2/api/s3/tokens/)
- [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)

## Support

If you encounter issues:
1. Check the application logs for detailed error messages
2. Verify your R2 configuration matches the examples above
3. Test your API tokens using the Cloudflare Dashboard
4. Review the Cloudflare R2 documentation for API-specific issues
