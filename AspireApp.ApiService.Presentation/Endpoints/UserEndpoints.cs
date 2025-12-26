using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Application.UseCases.Users;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Permissions;
using AspireApp.ApiService.Presentation.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers")
            .WithSummary("Get all users")
            .Produces<IEnumerable<UserDto>>(StatusCodes.Status200OK)
            .RequirePermission(PermissionNames.User.Read);

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get user by ID")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(PermissionNames.User.Read);

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .RequireAuthorization(); // Any authenticated user

        group.MapPut("/{id:guid}/permissions", AssignPermissionsToUser)
            .WithName("AssignPermissionsToUser")
            .WithSummary("Assign (replace) permissions to a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(PermissionNames.User.Write);

        group.MapPut("/{id:guid}/roles", AssignRoleToUser)
            .WithName("AssignRoleToUser")
            .WithSummary("Assign (replace) roles to a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(PermissionNames.User.Write);
    }

    private static async Task<IResult> GetAllUsers(
        [FromServices] GetAllUsersUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        [FromServices] GetUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetCurrentUser(
        [FromServices] GetUserUseCase useCase,
        System.Security.Claims.ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Result.Failure(DomainErrors.General.Unauthorized()).ToHttpResult();
        }

        var result = await useCase.ExecuteAsync(userId, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> AssignPermissionsToUser(
        Guid id,
        [FromBody] AssignPermissionsToUserRequest request,
        [FromServices] AssignPermissionsToUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> AssignRoleToUser(
        Guid id,
        [FromBody] AssignRoleToUserRequest request,
        [FromServices] AssignRoleToUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}

