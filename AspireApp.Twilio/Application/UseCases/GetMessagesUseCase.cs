using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Twilio.Application.DTOs;
using AspireApp.Twilio.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Twilio.Application.UseCases;

public class GetMessagesUseCase : BaseUseCase
{
    private readonly IMessageRepository _messageRepository;

    public GetMessagesUseCase(
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _messageRepository = messageRepository;
    }

    public async Task<Result<List<MessageDto>>> ExecuteAsync(GetMessagesRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _messageRepository.GetQueryableAsync();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                var normalizedPhone = dto.PhoneNumber.Replace(" ", "").Trim();
                queryable = queryable.Where(m => m.RecipientPhoneNumber == normalizedPhone);
            }

            if (dto.Channel.HasValue)
            {
                queryable = queryable.Where(m => m.Channel == dto.Channel.Value);
            }

            if (dto.Status.HasValue)
            {
                queryable = queryable.Where(m => m.Status == dto.Status.Value);
            }

            // Apply ordering
            queryable = queryable.OrderByDescending(m => m.CreationTime);

            // Apply pagination
            var totalCount = await queryable.CountAsync(cancellationToken);
            var messages = await queryable
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync(cancellationToken);

            var messageDtos = Mapper.Map<List<MessageDto>>(messages);

            return Result<List<MessageDto>>.Success(messageDtos, (long)totalCount, dto.PageNumber, dto.PageSize);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<MessageDto>>(DomainErrors.General.ServerError(ex.Message));
        }
    }
}

