using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Application.Users.UseCases;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.Domain.Shared.Interfaces;
using AutoMapper;
using NSubstitute;

namespace AspireApp.ApiService.Application.Tests.Users.UseCases;

public class GetAllUsersUseCaseTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly GetAllUsersUseCase _useCase;

    public GetAllUsersUseCaseTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = Substitute.For<IMapper>();
        _useCase = new GetAllUsersUseCase(_userRepository, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User("test1@test.com", "user1", PasswordHash.Create("h", "s"), "First1", "Last1"),
            new User("test2@test.com", "user2", PasswordHash.Create("h", "s"), "First2", "Last2")
        };
        var userDtos = new List<UserDto>
        {
            new UserDto(Guid.NewGuid(), "test1@test.com", "user1", "First1", "Last1", false, true, "en", new List<string>()),
            new UserDto(Guid.NewGuid(), "test2@test.com", "user2", "First2", "Last2", false, true, "en", new List<string>())
        };

        _userRepository.GetListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(users);
        _mapper.Map<IEnumerable<UserDto>>(users).Returns(userDtos);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
    }
}

