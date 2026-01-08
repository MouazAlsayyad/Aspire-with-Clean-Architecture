using AspireApp.ApiService.Application.ActivityLogs.DTOs;
using AspireApp.ApiService.Application.ActivityLogs.UseCases;
using AspireApp.ApiService.Domain.ActivityLogs.Entities;
using AspireApp.ApiService.Domain.ActivityLogs.Enums;
using AspireApp.ApiService.Domain.ActivityLogs.Interfaces;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.ActivityLogs.UseCases;

public class GetActivityLogsUseCaseTests
{
    private readonly IActivityLogStore _activityLogStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly GetActivityLogsUseCase _useCase;

    public GetActivityLogsUseCaseTests()
    {
        _activityLogStore = Substitute.For<IActivityLogStore>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _useCase = new GetActivityLogsUseCase(_activityLogStore, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReturnPagedResults()
    {
        // Arrange
        var request = new GetActivityLogsRequest { PageNumber = 1, PageSize = 10 };
        var activityLogs = new List<ActivityLog>
        {
            new ActivityLog("UserCreated", "User created", Guid.NewGuid(), "testuser")
        };
        var activityLogDtos = new List<ActivityLogDto>
        {
            new ActivityLogDto(Guid.NewGuid(), "UserCreated", "User created", null, Guid.NewGuid(), "testuser", null, null, null, null, null, ActivitySeverity.Info, true, null, DateTime.UtcNow)
        };

        _activityLogStore.GetListAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<ActivitySeverity?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<bool?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((activityLogs, 1));
        _mapper.Map<IEnumerable<ActivityLogDto>>(activityLogs).Returns(activityLogDtos);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Pagination.TotalCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPageNumber_ShouldNormalizeToOne()
    {
        // Arrange
        var request = new GetActivityLogsRequest { PageNumber = 0, PageSize = 10 };
        _activityLogStore.GetListAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<ActivitySeverity?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<bool?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((new List<ActivityLog>(), 0));
        _mapper.Map<IEnumerable<ActivityLogDto>>(Arg.Any<IEnumerable<ActivityLog>>()).Returns(new List<ActivityLogDto>());

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        await _activityLogStore.Received(1).GetListAsync(1, Arg.Any<int>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<ActivitySeverity?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<bool?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithLargePageSize_ShouldLimitTo100()
    {
        // Arrange
        var request = new GetActivityLogsRequest { PageNumber = 1, PageSize = 200 };
        _activityLogStore.GetListAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<ActivitySeverity?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<bool?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((new List<ActivityLog>(), 0));
        _mapper.Map<IEnumerable<ActivityLogDto>>(Arg.Any<IEnumerable<ActivityLog>>()).Returns(new List<ActivityLogDto>());

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        await _activityLogStore.Received(1).GetListAsync(Arg.Any<int>(), 100, Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<ActivitySeverity?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<bool?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}

