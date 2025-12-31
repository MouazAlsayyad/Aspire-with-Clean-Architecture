using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.FileUpload;
using AspireApp.ApiService.Application.UseCases.FileUpload;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Enums;
using AspireApp.ApiService.Domain.Permissions;
using AspireApp.ApiService.Presentation.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspireApp.ApiService.Presentation.Endpoints;

public static class FileUploadEndpoints
{
    public static void MapFileUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files")
            .WithTags("FileUpload")
            .WithValidation();

        group.MapPost("/upload", UploadFile)
            .WithName("UploadFile")
            .WithSummary("Upload a file. Set useBackgroundQueue=true to process upload asynchronously for faster response times.")
            .DisableAntiforgery() // File uploads typically don't use antiforgery tokens
            .Produces<FileUploadDto>(StatusCodes.Status201Created)
            .Produces<FileUploadQueuedDto>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(PermissionNames.FileUpload.Write);

        group.MapGet("/", GetAllFiles)
            .WithName("GetAllFiles")
            .WithSummary("Get all file uploads")
            .Produces<IEnumerable<FileUploadDto>>(StatusCodes.Status200OK)
            .RequirePermission(PermissionNames.FileUpload.Read);

        group.MapGet("/{id:guid}", GetFileUpload)
            .WithName("GetFileUpload")
            .WithSummary("Get file upload metadata by ID")
            .Produces<FileUploadDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(PermissionNames.FileUpload.Read);

        group.MapGet("/{id:guid}/download", DownloadFile)
            .WithName("DownloadFile")
            .WithSummary("Download a file")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(PermissionNames.FileUpload.Read);

        group.MapDelete("/{id:guid}", DeleteFile)
            .WithName("DeleteFile")
            .WithSummary("Delete a file upload")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(PermissionNames.FileUpload.Delete);
    }

    private static async Task<IResult> UploadFile(
        [FromForm] UploadFileFormDto formDto,
        [FromServices] UploadFileUseCase useCase,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (formDto.File == null || formDto.File.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided or file is empty." });
        }

        // Get user ID from claims
        Guid? userId = null;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        // Parse UseBackgroundQueue string to boolean (forgiving parsing)
        bool useBackgroundQueue = false;
        if (!string.IsNullOrWhiteSpace(formDto.UseBackgroundQueue))
        {
            var value = formDto.UseBackgroundQueue.Trim();
            useBackgroundQueue = value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                 value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                                 value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        var request = new UploadFileRequest
        {
            StorageType = formDto.StorageType ?? FileStorageType.FileSystem,
            Description = formDto.Description,
            Tags = formDto.Tags,
            UseBackgroundQueue = useBackgroundQueue
        };

        var result = await useCase.ExecuteAsync(
            formDto.File.FileName,
            formDto.File.ContentType,
            formDto.File.OpenReadStream(),
            formDto.File.Length,
            request,
            userId,
            cancellationToken);

        if (result.IsSuccess)
        {
            // If background queue is enabled, return a simple queued response
            if (request.UseBackgroundQueue)
            {
                var queuedResponse = new FileUploadQueuedDto
                {
                    FileId = result.Value.Id,
                    FileName = result.Value.FileName,
                    Message = "File upload has been queued and will be processed in the background. Please check the file status later."
                };
                return Results.Accepted($"/api/files/{result.Value.Id}", queuedResponse);
            }

            // Otherwise, return the full file upload details
            return Results.Created($"/api/files/{result.Value.Id}", result.Value);
        }

        return result.ToHttpResult();
    }

    private static async Task<IResult> GetAllFiles(
        [FromServices] GetAllFileUploadsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetFileUpload(
        Guid id,
        [FromServices] GetFileUploadUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DownloadFile(
        Guid id,
        [FromServices] GetFileUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return result.ToHttpResult();
        }

        var downloadDto = result.Value;
        return Results.File(
            downloadDto.FileStream,
            downloadDto.ContentType,
            downloadDto.FileName);
    }

    private static async Task<IResult> DeleteFile(
        Guid id,
        [FromServices] DeleteFileUseCase useCase,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // Get user ID from claims
        Guid? userId = null;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await useCase.ExecuteAsync(id, userId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.ToHttpResult();
    }
}

