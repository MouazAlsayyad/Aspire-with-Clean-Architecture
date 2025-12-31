using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.Users.Services;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Application.UseCases.Users;

public class CreateUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserManager _userManager;

    public CreateUserUseCase(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IUserManager userManager,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
    }

    public async Task<Result<UserDto>> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            try
            {
                // Hash password
                var (hash, salt) = _passwordHasher.HashPassword(request.Password);
                var passwordHash = new PasswordHash(hash, salt);

                // Use domain service to create user
                var user = await _userManager.CreateAsync(
                    request.Email,
                    request.UserName,
                    passwordHash,
                    request.FirstName,
                    request.LastName,
                    ct);

                // Set IsActive if provided, otherwise defaults to true
                if (request.IsActive.HasValue && !request.IsActive.Value)
                {
                    _userManager.Deactivate(user);
                }

                // Assign default User role
                var defaultRole = await _roleRepository.GetByNameAsync("User", ct);
                if (defaultRole != null)
                {
                    await _userManager.AssignRoleAsync(user, defaultRole.Id, ct);
                }

                // Save user
                var savedUser = await _userRepository.InsertAsync(user, ct);

                // Map to DTO
                var userDto = Mapper.Map<UserDto>(savedUser);

                return Result.Success(userDto);
            }
            catch (DomainException ex)
            {
                return Result.Failure<UserDto>(ex.Error);
            }
        }, cancellationToken);
    }
}

