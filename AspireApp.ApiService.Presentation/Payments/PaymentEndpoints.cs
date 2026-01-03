using AspireApp.Payment.Application.DTOs;
using AspireApp.Payment.Application.UseCases;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AspireApp.ApiService.Presentation.Payments;

/// <summary>
/// Payment API endpoints
/// </summary>
public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPaymentEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreatePayment)
            .WithName("CreatePayment")
            .WithSummary("Creates a new payment")
            .Produces<PaymentResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/process", ProcessPayment)
            .WithName("ProcessPayment")
            .WithSummary("Processes an existing payment")
            .Produces<PaymentResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/refund", RefundPayment)
            .WithName("RefundPayment")
            .WithSummary("Refunds a payment")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetPayment)
            .WithName("GetPayment")
            .WithSummary("Gets a payment by ID")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/history", GetPaymentHistory)
            .WithName("GetPaymentHistory")
            .WithSummary("Gets payment transaction history")
            .Produces<IEnumerable<PaymentTransactionDto>>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> CreatePayment(
        [FromBody] CreatePaymentDto dto,
        [FromServices] CreatePaymentUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> ProcessPayment(
        [FromRoute] Guid id,
        [FromBody] ProcessPaymentDto dto,
        [FromServices] ProcessPaymentUseCase useCase,
        CancellationToken cancellationToken)
    {
        dto.PaymentId = id;
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> RefundPayment(
        [FromRoute] Guid id,
        [FromBody] RefundPaymentDto dto,
        [FromServices] RefundPaymentUseCase useCase,
        CancellationToken cancellationToken)
    {
        dto.PaymentId = id;
        var result = await useCase.ExecuteAsync(dto, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error?.Code == "NOT_FOUND"
                ? Results.NotFound(result.Error)
                : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetPayment(
        [FromRoute] Guid id,
        [FromServices] GetPaymentUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> GetPaymentHistory(
        [FromRoute] Guid id,
        [FromServices] GetPaymentHistoryUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}

