using AspireApp.ApiService.Presentation.Extensions;
using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Emails;

public static class EmailEndpoints
{
    public static void MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/emails")
            .WithTags("Emails")
            .WithValidation()
            .RequireAuthorization(); // All endpoints require authentication

        group.MapPost("/booking", SendBookingEmail)
            .WithName("SendBookingEmail")
            .WithSummary("Send booking confirmation email")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/completed-booking", SendCompletedBookingEmail)
            .WithName("SendCompletedBookingEmail")
            .WithSummary("Send completed booking notification email (for tenants)")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/membership", SendMembershipEmail)
            .WithName("SendMembershipEmail")
            .WithSummary("Send membership subscription email")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/otp", SendOTPEmail)
            .WithName("SendOTPEmail")
            .WithSummary("Send OTP verification email")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/payout-otp", SendPayoutOTPEmail)
            .WithName("SendPayoutOTPEmail")
            .WithSummary("Send payout OTP verification email")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/payout-confirmation", SendPayoutConfirmationEmail)
            .WithName("SendPayoutConfirmationEmail")
            .WithSummary("Send payout confirmation email with attachments")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/payout-rejection", SendPayoutRejectionEmail)
            .WithName("SendPayoutRejectionEmail")
            .WithSummary("Send payout rejection email")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/password-reset", SendPasswordResetEmail)
            .WithName("SendPasswordResetEmail")
            .WithSummary("Send password reset email")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/onboarding", SendOnboardingEmail)
            .WithName("SendOnboardingEmail")
            .WithSummary("Send Stripe onboarding email")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/subscription", SendSubscriptionEmail)
            .WithName("SendSubscriptionEmail")
            .WithSummary("Send subscription invoice email")
            .Produces<EmailLogDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/logs", GetEmailLogs)
            .WithName("GetEmailLogs")
            .WithSummary("Get paginated email logs (admin only)")
            .Produces<GetEmailLogsResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SendBookingEmail(
        [FromBody] SendBookingEmailDto dto,
        [FromServices] SendBookingEmailUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendCompletedBookingEmail(
        [FromBody] SendCompletedBookingEmailDto dto,
        [FromServices] SendCompletedBookingEmailUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendMembershipEmail(
        [FromBody] SendMembershipEmailDto dto,
        [FromServices] SendMembershipEmailUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendOTPEmail(
        [FromBody] SendOTPEmailDto dto,
        [FromServices] SendOTPEmailUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendPayoutOTPEmail(
        [FromBody] SendPayoutOTPDto dto,
        [FromServices] SendPayoutOTPUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendPayoutConfirmationEmail(
        [FromBody] SendPayoutConfirmationDto dto,
        [FromServices] SendPayoutConfirmationUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendPayoutRejectionEmail(
        [FromBody] SendPayoutRejectionDto dto,
        [FromServices] SendPayoutRejectionUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendPasswordResetEmail(
        [FromBody] SendPasswordResetDto dto,
        [FromServices] SendPasswordResetUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendOnboardingEmail(
        [FromBody] SendOnboardingEmailDto dto,
        [FromServices] SendOnboardingEmailUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SendSubscriptionEmail(
        [FromBody] SendSubscriptionEmailDto dto,
        [FromServices] SendSubscriptionEmailUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetEmailLogs(
        [AsParameters] GetEmailLogsRequestDto request,
        [FromServices] GetEmailLogsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}

