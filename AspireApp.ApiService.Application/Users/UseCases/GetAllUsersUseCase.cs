using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.Users.DTOs;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;

namespace AspireApp.ApiService.Application.Users.UseCases;

public class GetAllUsersUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersUseCase(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<UserDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetListAsync(includeDeleted: false, cancellationToken);
        var userDtos = Mapper.Map<IEnumerable<UserDto>>(users);
        return Result.Success(userDtos);
    }
}

