using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Services;

/// <summary>
/// Domain service (Manager) for User entity.
/// Handles user-related domain logic and business rules.
/// </summary>
public class UserManager : DomainService, IUserManager
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserManager(
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// Creates a new user with domain validation
    /// </summary>
    public async Task<User> CreateAsync(
        string email,
        string userName,
        PasswordHash passwordHash,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        // Domain validation: Check if email already exists
        if (await _userRepository.ExistsAsync(email, cancellationToken))
        {
            throw new InvalidOperationException($"User with email '{email}' already exists.");
        }

        // Domain validation: Check if username already exists
        var existingUser = await _userRepository.GetByUserNameAsync(userName, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with username '{userName}' already exists.");
        }

        // Create user entity
        var user = new User(email, userName, passwordHash, firstName, lastName);

        return user;
    }

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    public async Task AssignRoleAsync(User user, string roleName, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' not found.");
        }

        user.AddRole(role);
    }

    /// <summary>
    /// Assigns a role to a user by role ID
    /// </summary>
    public async Task AssignRoleAsync(User user, Guid roleId, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var role = await _roleRepository.GetAsync(roleId, cancellationToken: cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
        }

        user.AddRole(role);
    }

    /// <summary>
    /// Removes a role from a user
    /// </summary>
    public void RemoveRole(User user, Guid roleId)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.RemoveRole(roleId);
    }

    /// <summary>
    /// Changes user password
    /// </summary>
    public void ChangePassword(User user, PasswordHash newPasswordHash)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.UpdatePassword(newPasswordHash);
    }

    /// <summary>
    /// Activates a user account
    /// </summary>
    public void Activate(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.Activate();
    }

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    public void Deactivate(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.Deactivate();
    }

    /// <summary>
    /// Confirms user email
    /// </summary>
    public void ConfirmEmail(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.ConfirmEmail();
    }

    /// <summary>
    /// Validates if user can be deleted (domain rules)
    /// </summary>
    public void ValidateDeletion(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Add domain-specific validation rules here
        // For example: Cannot delete admin users, etc.
    }
}

