using AspireApp.Domain.Shared.Common;
using AspireApp.Notifications.Application.DTOs;
using AspireApp.Notifications.Domain.Models;
using AspireApp.Notifications.Domain.Services;
using AutoMapper;
using FluentValidation;

namespace AspireApp.Notifications.Application.UseCases;

/// <summary>
/// Use case for sending notifications via multiple channels
/// </summary>
public class SendNotificationUseCase
{
    private readonly NotificationOrchestrator _orchestrator;
    private readonly IMapper _mapper;
    private readonly IValidator<SendNotificationDto> _validator;

    public SendNotificationUseCase(
        NotificationOrchestrator orchestrator,
        IMapper mapper,
        IValidator<SendNotificationDto> validator)
    {
        _orchestrator = orchestrator;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<IEnumerable<NotificationResultDto>>> ExecuteAsync(
        SendNotificationDto dto,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<IEnumerable<NotificationResultDto>>(
                DomainErrors.General.InvalidInput(validationResult.Errors.First().ErrorMessage));
        }

        // Map DTO to domain model
        var request = _mapper.Map<NotificationRequest>(dto);

        // Send notifications
        var results = await _orchestrator.SendAsync(request, cancellationToken);

        // Map results to DTOs
        var resultDtos = _mapper.Map<IEnumerable<NotificationResultDto>>(results);

        return Result.Success(resultDtos);
    }
}

