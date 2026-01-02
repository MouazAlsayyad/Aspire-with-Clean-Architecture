using AspireApp.Domain.Shared.Common;
using AspireApp.ApiService.Application.Roles.DTOs;
using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Application.Users.UseCases;
using AspireApp.ApiService.Domain.Permissions;
using AspireApp.ApiService.Presentation.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithValidation();

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

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .RequirePermission(PermissionNames.User.Write);

        group.MapPut("/{id:guid}/activation", ToggleUserActivation)
            .WithName("ToggleUserActivation")
            .WithSummary("Toggle user activation status (activate or deactivate)")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(PermissionNames.User.Write);

        group.MapPut("/{id:guid}/password", UpdatePassword)
            .WithName("UpdatePassword")
            .WithSummary("Update user password")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(); // Any authenticated user can update their own password

        group.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update user information")
            .Produces<UserDto>(StatusCodes.Status200OK)
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

    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        [FromServices] CreateUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return result.ToHttpCreatedResult($"/api/users/{result.Value.Id}");
        }
        return result.ToHttpResult();
    }

    private static async Task<IResult> ToggleUserActivation(
        Guid id,
        [FromBody] ToggleUserActivationRequest request,
        [FromServices] ToggleUserActivationUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdatePassword(
        Guid id,
        [FromBody] UpdatePasswordRequest request,
        [FromServices] UpdatePasswordUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] UpdateUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}

