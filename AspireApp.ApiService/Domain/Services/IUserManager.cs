using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Services;

/// <summary>
/// Interface for User domain service (Manager).
/// Handles user-related domain logic and business rules.
/// </summary>
public interface IUserManager : IDomainService
{
    /// <summary>
    /// Creates a new user with domain validation
    /// </summary>
    Task<User> CreateAsync(
        string email,
        string userName,
        PasswordHash passwordHash,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    Task AssignRoleAsync(User user, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user by role ID
    /// </summary>
    Task AssignRoleAsync(User user, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user
    /// </summary>
    void RemoveRole(User user, Guid roleId);

    /// <summary>
    /// Changes user password
    /// </summary>
    void ChangePassword(User user, PasswordHash newPasswordHash);

    /// <summary>
    /// Activates a user account
    /// </summary>
    void Activate(User user);

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    void Deactivate(User user);

    /// <summary>
    /// Confirms user email
    /// </summary>
    void ConfirmEmail(User user);

    /// <summary>
    /// Validates if user can be deleted (domain rules)
    /// </summary>
    void ValidateDeletion(User user);
}

