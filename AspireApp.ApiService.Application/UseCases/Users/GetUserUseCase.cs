using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.Users;

public class GetUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;

    public GetUserUseCase(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(userId, includeDeleted: false, cancellationToken);
        if (user == null)
        {
            return DomainErrors.User.NotFound(userId);
        }

        var userDto = Mapper.Map<UserDto>(user);
        return Result.Success(userDto);
    }
}

