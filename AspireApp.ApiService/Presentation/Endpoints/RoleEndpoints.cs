using AspireApp.ApiService.Application.DTOs.Role;
using AspireApp.ApiService.Application.UseCases.Roles;
using AspireApp.ApiService.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles").WithTags("Roles");

        group.MapGet("/", GetAllRoles)
            .WithName("GetAllRoles")
            .WithSummary("Get all roles")
            .Produces<IEnumerable<RoleDto>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetRoleById)
            .WithName("GetRoleById")
            .WithSummary("Get role by ID")
            .Produces<RoleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPost("/", CreateRole)
            .WithName("CreateRole")
            .WithSummary("Create a new role")
            .Produces<RoleDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminOnly");

        group.MapPut("/{id:guid}", UpdateRole)
            .WithName("UpdateRole")
            .WithSummary("Update a role")
            .Produces<RoleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        group.MapDelete("/{id:guid}", DeleteRole)
            .WithName("DeleteRole")
            .WithSummary("Delete a role")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");
    }

    private static async Task<IResult> GetAllRoles(
        GetAllRolesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetRoleById(
        Guid id,
        GetRoleUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        CreateRoleUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        
        if (result.IsSuccess)
        {
            return result.ToHttpCreatedResult($"/api/roles/{result.Value.Id}");
        }
        
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        UpdateRoleUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteRole(
        Guid id,
        DeleteRoleUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }
        return result.ToHttpResult();
    }
}

