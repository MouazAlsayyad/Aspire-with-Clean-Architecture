using AspireApp.FirebaseNotifications.Application.DTOs;
using AspireApp.FirebaseNotifications.Application.UseCases;
using AspireApp.ApiService.Presentation.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Notifications;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .WithValidation()
            .RequireAuthorization(); // All endpoints require authentication

        group.MapPost("/", CreateNotification)
            .WithName("CreateNotification")
            .WithSummary("Create a new notification")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetNotifications)
            .WithName("GetNotifications")
            .WithSummary("Get paginated notifications for current user")
            .Produces<GetNotificationsResponseDto>(StatusCodes.Status200OK);

        group.MapPut("/{id:guid}/read", MarkAsRead)
            .WithName("MarkNotificationAsRead")
            .WithSummary("Mark a notification as read")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/unread", MarkAsUnread)
            .WithName("MarkNotificationAsUnread")
            .WithSummary("Mark a notification as unread")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/mark-all-read", MarkAllAsRead)
            .WithName("MarkAllNotificationsAsRead")
            .WithSummary("Mark all notifications as read for current user")
            .Produces<int>(StatusCodes.Status200OK);

        group.MapPost("/register-fcm-token", RegisterFCMToken)
            .WithName("RegisterFCMToken")
            .WithSummary("Register FCM token for push notifications")
            .Produces<RegisterFCMTokenResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/has-fcm-token", HasFCMToken)
            .WithName("HasFCMToken")
            .WithSummary("Check if current user has FCM token registered")
            .Produces<bool>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateNotification(
        [FromBody] CreateNotificationDto dto,
        [FromServices] CreateNotificationUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/notifications/{dto.UserId}", result)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetNotifications(
        [AsParameters] GetNotificationsRequestDto request,
        [FromServices] GetNotificationsUseCase useCase,
        System.Security.Claims.ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await useCase.ExecuteAsync(userId, request, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> MarkAsRead(
        Guid id,
        [FromServices] UpdateNotificationStatusUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, true, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> MarkAsUnread(
        Guid id,
        [FromServices] UpdateNotificationStatusUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, false, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> MarkAllAsRead(
        [FromServices] MarkAllNotificationsAsReadUseCase useCase,
        System.Security.Claims.ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await useCase.ExecuteAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> RegisterFCMToken(
        [FromBody] RegisterFCMTokenDto dto,
        [FromServices] RegisterFCMTokenUseCase useCase,
        System.Security.Claims.ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await useCase.ExecuteAsync(userId, dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> HasFCMToken(
        [FromServices] HasFCMTokenUseCase useCase,
        System.Security.Claims.ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await useCase.ExecuteAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}

