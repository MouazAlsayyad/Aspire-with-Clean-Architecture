using AspireApp.ApiService.Application.DTOs.Auth;
using AspireApp.ApiService.Application.UseCases.Authentication;
using AspireApp.ApiService.Presentation.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user")
            .Produces<Application.DTOs.User.UserDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .AllowAnonymous();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login and get access token")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .AllowAnonymous();

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token using refresh token")
            .WithDescription("Exchanges a valid refresh token for a new access token and refresh token pair. " +
                           "The old refresh token will be revoked and cannot be reused. " +
                           "If a revoked token is detected, all tokens for the user will be revoked for security.")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        RegisterUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        
        if (result.IsSuccess)
        {
            return result.ToHttpCreatedResult($"/api/users/{result.Value.Id}");
        }
        
        return result.ToHttpResult();
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        LoginUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        RefreshTokenUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.ToHttpResult();
    }
}

