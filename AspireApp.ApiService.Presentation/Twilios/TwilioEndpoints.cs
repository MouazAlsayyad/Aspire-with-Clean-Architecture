using AspireApp.Domain.Shared.Common;
using AspireApp.ApiService.Presentation.Extensions;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Application.UseCases;
using AspireApp.Twilio.Domain.Enums;
using AspireApp.Twilio.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.ApiService.Presentation.Twilios;

public static class TwilioEndpoints
{
    public static void MapTwilioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/twilio")
            .WithTags("Twilio")
            .WithValidation();

        // Public endpoints for sending messages
        group.MapPost("/sms", SendSms)
            .WithName("SendSms")
            .WithSummary("Send an SMS message")
            .Produces<MessageDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("/whatsapp", SendWhatsApp)
            .WithName("SendWhatsApp")
            .WithSummary("Send a WhatsApp message")
            .Produces<MessageDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("/otp", SendOtp)
            .WithName("SendOtp")
            .WithSummary("Generate and send an OTP code")
            .Produces<MessageDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("/otp/validate", ValidateOtp)
            .WithName("ValidateOtp")
            .WithSummary("Validate an OTP code")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapGet("/messages", GetMessages)
            .WithName("GetMessages")
            .WithSummary("Get message history")
            .Produces<List<MessageDto>>(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Webhook endpoint for Twilio status callbacks (must be public)
        group.MapPost("/whatsapp-status", HandleWhatsAppStatus)
            .WithName("WhatsAppStatusWebhook")
            .WithSummary("Twilio webhook for WhatsApp message status updates")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();
    }

    private static async Task<IResult> SendSms(
        [FromBody] SendSmsDto dto,
        [FromServices] SendSmsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> SendWhatsApp(
        [FromBody] SendWhatsAppDto dto,
        [FromServices] SendWhatsAppUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> SendOtp(
        [FromBody] SendOtpDto dto,
        [FromServices] SendOtpUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ValidateOtp(
        [FromBody] ValidateOtpDto dto,
        [FromServices] ValidateOtpUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetMessages(
        [AsParameters] GetMessagesRequestDto dto,
        [FromServices] GetMessagesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> HandleWhatsAppStatus(
        HttpContext context,
        ITwilioSmsManager twilioSmsManager,
        CancellationToken cancellationToken)
    {
        try
        {
            // Read form data from Twilio webhook
            var form = await context.Request.ReadFormAsync(cancellationToken);
            var messageSid = form["MessageSid"].ToString();
            var messageStatus = form["MessageStatus"].ToString();

            if (string.IsNullOrWhiteSpace(messageSid))
            {
                return Results.BadRequest("MessageSid is required");
            }

            // Map Twilio status to our MessageStatus enum
            var status = MapTwilioStatus(messageStatus);
            var errorMessage = form["ErrorMessage"].ToString();
            var failureReason = status == MessageStatus.Failed 
                ? (string.IsNullOrWhiteSpace(errorMessage) ? "Unknown error" : errorMessage)
                : null;

            // Update message status
            await twilioSmsManager.UpdateMessageStatusAsync(messageSid, status, failureReason, cancellationToken);

            // If WhatsApp failed, trigger SMS fallback
            if (status == MessageStatus.Failed)
            {
                await twilioSmsManager.HandleWhatsAppFailureAsync(messageSid, failureReason ?? "Delivery failed", cancellationToken);
            }

            // Twilio expects a TwiML response or 200 OK
            return Results.Ok();
        }
        catch (Exception)
        {
            // Log error but return 200 OK to Twilio (they'll retry if needed)
            return Results.Ok();
        }
    }

    private static MessageStatus MapTwilioStatus(string? twilioStatus)
    {
        return twilioStatus?.ToLower() switch
        {
            "queued" => MessageStatus.Queued,
            "sent" => MessageStatus.Sent,
            "delivered" => MessageStatus.Delivered,
            "failed" or "undelivered" => MessageStatus.Failed,
            _ => MessageStatus.Queued
        };
    }
}

