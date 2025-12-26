using AspireApp.ApiService.Application.DTOs.Permission;
using AspireApp.ApiService.Application.UseCases.Permissions;
using AspireApp.ApiService.Presentation.Extensions;

namespace AspireApp.ApiService.Presentation.Endpoints;

public static class PermissionEndpoints
{
    public static void MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/permissions").WithTags("Permissions");

        group.MapGet("/", GetAllPermissions)
            .WithName("GetAllPermissions")
            .WithSummary("Get all permissions")
            .Produces<IEnumerable<PermissionDto>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetPermissionById)
            .WithName("GetPermissionById")
            .WithSummary("Get permission by ID")
            .Produces<PermissionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapGet("/resource/{resource}", GetPermissionsByResource)
            .WithName("GetPermissionsByResource")
            .WithSummary("Get permissions by resource")
            .Produces<IEnumerable<PermissionDto>>(StatusCodes.Status200OK)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAllPermissions(
        GetAllPermissionsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetPermissionById(
        Guid id,
        GetPermissionUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetPermissionsByResource(
        string resource,
        GetPermissionsByResourceUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(resource, cancellationToken);
        return result.ToHttpResult();
    }
}

