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
            .WithSummary("Upload a file")
            .DisableAntiforgery() // File uploads typically don't use antiforgery tokens
            .Produces<FileUploadDto>(StatusCodes.Status201Created)
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
        [FromForm] IFormFile file,
        [FromForm] FileStorageType? storageType,
        [FromForm] string? description,
        [FromForm] string? tags,
        [FromServices] UploadFileUseCase useCase,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
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

        var request = new UploadFileRequest
        {
            StorageType = storageType ?? FileStorageType.FileSystem,
            Description = description,
            Tags = tags
        };

        var result = await useCase.ExecuteAsync(
            file.FileName,
            file.ContentType,
            file.OpenReadStream(),
            file.Length,
            request,
            userId,
            cancellationToken);

        if (result.IsSuccess)
        {
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

