using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.Auth;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Application.UseCases.Authentication;

public class RegisterUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            // Check if user already exists
            if (await _userRepository.ExistsAsync(request.Email, ct))
            {
                return DomainErrors.User.EmailAlreadyExists(request.Email);
            }

            // Hash password
            var (hash, salt) = _passwordHasher.HashPassword(request.Password);
            var passwordHash = new PasswordHash(hash, salt);

            // Create user
            var user = new User(
                request.Email,
                request.UserName,
                passwordHash,
                request.FirstName,
                request.LastName
            );

            // Assign default User role
            var defaultRole = await _roleRepository.GetByNameAsync("User", ct);
            if (defaultRole != null)
            {
                user.AddRole(defaultRole);
            }

            // Save user
            var savedUser = await _userRepository.InsertAsync(user, ct);

            // Map to DTO
            var userDto = Mapper.Map<UserDto>(savedUser);

            return Result.Success(userDto);
        }, cancellationToken);
    }
}
