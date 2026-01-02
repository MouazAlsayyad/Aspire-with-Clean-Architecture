using AspireApp.Domain.Shared.Common;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.Email.Application.DTOs;
using AspireApp.Email.Domain.Interfaces;
using AutoMapper;

namespace AspireApp.Email.Application.UseCases;

public class GetEmailLogsUseCase : BaseUseCase
{
    private readonly IEmailLogRepository _emailLogRepository;

    public GetEmailLogsUseCase(
        IEmailLogRepository emailLogRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _emailLogRepository = emailLogRepository;
    }

    public async Task<Result<GetEmailLogsResponseDto>> ExecuteAsync(
        GetEmailLogsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (emailLogs, totalCount) = await _emailLogRepository.GetEmailLogsAsync(
                request.EmailType,
                request.Status,
                request.FromDate,
                request.ToDate,
                request.Skip,
                request.Take,
                cancellationToken);

            var emailLogDtos = Mapper.Map<List<EmailLogDto>>(emailLogs);

            var response = new GetEmailLogsResponseDto
            {
                EmailLogs = emailLogDtos,
                TotalCount = totalCount,
                Skip = request.Skip,
                Take = request.Take
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<GetEmailLogsResponseDto>(
                DomainErrors.General.InternalError($"Failed to get email logs: {ex.Message}"));
        }
    }
}

