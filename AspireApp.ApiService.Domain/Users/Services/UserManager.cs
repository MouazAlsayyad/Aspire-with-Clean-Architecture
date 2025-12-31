using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Notifications.Interfaces;
using AspireApp.ApiService.Domain.Permissions.Entities;
using AspireApp.ApiService.Domain.Permissions.Interfaces;
using AspireApp.ApiService.Domain.Roles.Entities;
using AspireApp.ApiService.Domain.Roles.Interfaces;
using AspireApp.ApiService.Domain.Services;
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Users.Services;

/// <summary>
/// Domain service (Manager) for User entity.
/// Handles user-related domain logic and business rules.
/// Throws DomainException for business rule violations.
/// </summary>
public class UserManager : DomainService, IUserManager
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IFirebaseAuthService? _firebaseAuthService;

    public UserManager(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IFirebaseAuthService? firebaseAuthService = null)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _firebaseAuthService = firebaseAuthService;
    }

    /// <summary>
    /// Creates a new user.
    /// Note: Email and username uniqueness validation is handled by FluentValidation at the application layer.
    /// </summary>
    public async Task<User> CreateAsync(
        string email,
        string userName,
        PasswordHash passwordHash,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        // Create user entity
        // Validation for email/username uniqueness is handled by FluentValidation validators
        var user = new User(email, userName, passwordHash, firstName, lastName);

        return user;
    }

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    public async Task AssignRoleAsync(User user, string roleName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken)
            ?? throw new DomainException(DomainErrors.Role.NotFound(roleName));
        user.AddRole(role);
    }

    /// <summary>
    /// Assigns a role to a user by role ID
    /// </summary>
    public async Task AssignRoleAsync(User user, Guid roleId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var role = await _roleRepository.GetAsync(roleId, cancellationToken: cancellationToken)
            ?? throw new DomainException(DomainErrors.Role.NotFound(roleId));
        user.AddRole(role);
    }

    /// <summary>
    /// Removes a role from a user
    /// </summary>
    public void RemoveRole(User user, Guid roleId)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.RemoveRole(roleId);
    }

    /// <summary>
    /// Changes user password
    /// </summary>
    public void ChangePassword(User user, PasswordHash newPasswordHash)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.UpdatePassword(newPasswordHash);
    }

    /// <summary>
    /// Activates a user account
    /// </summary>
    public void Activate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Activate();
    }

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    public void Deactivate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Deactivate();
    }

    /// <summary>
    /// Confirms user email
    /// </summary>
    public void ConfirmEmail(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.ConfirmEmail();
    }

    /// <summary>
    /// Validates if user can be deleted (domain rules)
    /// </summary>
    public void ValidateDeletion(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Add domain-specific validation rules here
        // For example: Cannot delete admin users, etc.
    }

    /// <summary>
    /// Checks if email already exists
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.ExistsAsync(email, cancellationToken);
    }

    /// <summary>
    /// Checks if username already exists
    /// </summary>
    public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByUserNameAsync(userName, cancellationToken);
        return existingUser != null;
    }

    /// <summary>
    /// Assigns a permission directly to a user
    /// </summary>
    public async Task AssignPermissionAsync(User user, string permissionName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var permission = await _permissionRepository.GetByNameAsync(permissionName, cancellationToken)
            ?? throw new DomainException(DomainErrors.Permission.NotFound(permissionName));
        user.AddPermission(permission);
    }

    /// <summary>
    /// Assigns a permission directly to a user by permission ID
    /// </summary>
    public async Task AssignPermissionAsync(User user, Guid permissionId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var permission = await _permissionRepository.GetAsync(permissionId, cancellationToken: cancellationToken)
            ?? throw new DomainException(DomainErrors.Permission.NotFound(permissionId));
        user.AddPermission(permission);
    }

    /// <summary>
    /// Removes a permission directly from a user
    /// </summary>
    public void RemovePermission(User user, Guid permissionId)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.RemovePermission(permissionId);
    }

    /// <summary>
    /// Assigns multiple permissions directly to a user by permission IDs
    /// </summary>
    public async Task AssignPermissionsAsync(User user, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        ArgumentNullException.ThrowIfNull(permissionIds);

        var permissionIdList = permissionIds.ToList();
        if (!permissionIdList.Any())
            return; // Nothing to assign

        // Fetch all permissions at once
        var permissions = new List<Permission>();
        foreach (var permissionId in permissionIdList)
        {
            var permission = await _permissionRepository.GetAsync(permissionId, cancellationToken: cancellationToken)
                ?? throw new DomainException(DomainErrors.Permission.NotFound(permissionId));
            permissions.Add(permission);
        }

        // Assign all permissions
        foreach (var permission in permissions)
        {
            user.AddPermission(permission);
        }
    }

    /// <summary>
    /// Assigns multiple permissions directly to a user by permission names
    /// </summary>
    public async Task AssignPermissionsAsync(User user, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        ArgumentNullException.ThrowIfNull(permissionNames);

        var permissionNameList = permissionNames.ToList();
        if (!permissionNameList.Any())
            return; // Nothing to assign

        // Fetch all permissions at once
        var permissions = new List<Permission>();
        foreach (var permissionName in permissionNameList)
        {
            var permission = await _permissionRepository.GetByNameAsync(permissionName, cancellationToken)
                ?? throw new DomainException(DomainErrors.Permission.NotFound(permissionName));
            permissions.Add(permission);
        }

        // Assign all permissions
        foreach (var permission in permissions)
        {
            user.AddPermission(permission);
        }
    }

    /// <summary>
    /// Sets (replaces) all permissions directly assigned to a user by permission IDs.
    /// This removes all existing direct permissions and assigns only the new ones.
    /// </summary>
    public async Task SetPermissionsAsync(User user, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        ArgumentNullException.ThrowIfNull(permissionIds);

        var permissionIdList = permissionIds.ToList();

        // If empty list, just clear all permissions
        if (!permissionIdList.Any())
        {
            user.SetPermissions(Enumerable.Empty<Permission>());
            return;
        }

        // Fetch all permissions at once
        var permissions = new List<Permission>();
        foreach (var permissionId in permissionIdList)
        {
            var permission = await _permissionRepository.GetAsync(permissionId, cancellationToken: cancellationToken)
                ?? throw new DomainException(DomainErrors.Permission.NotFound(permissionId));
            permissions.Add(permission);
        }

        // Replace all permissions
        user.SetPermissions(permissions);
    }

    /// <summary>
    /// Sets (replaces) all permissions directly assigned to a user by permission names.
    /// This removes all existing direct permissions and assigns only the new ones.
    /// </summary>
    public async Task SetPermissionsAsync(User user, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        ArgumentNullException.ThrowIfNull(permissionNames);

        var permissionNameList = permissionNames.ToList();

        // If empty list, just clear all permissions
        if (!permissionNameList.Any())
        {
            user.SetPermissions(Enumerable.Empty<Permission>());
            return;
        }

        // Fetch all permissions at once
        var permissions = new List<Permission>();
        foreach (var permissionName in permissionNameList)
        {
            var permission = await _permissionRepository.GetByNameAsync(permissionName, cancellationToken)
                ?? throw new DomainException(DomainErrors.Permission.NotFound(permissionName));
            permissions.Add(permission);
        }

        // Replace all permissions
        user.SetPermissions(permissions);
    }

    /// <summary>
    /// Assigns multiple roles to a user by role IDs
    /// </summary>
    public async Task AssignRolesAsync(User user, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        ArgumentNullException.ThrowIfNull(roleIds);

        var roleIdList = roleIds.ToList();
        if (!roleIdList.Any())
            return; // Nothing to assign

        // Fetch all roles at once
        var roles = new List<Role>();
        foreach (var roleId in roleIdList)
        {
            var role = await _roleRepository.GetAsync(roleId, cancellationToken: cancellationToken)
                ?? throw new DomainException(DomainErrors.Role.NotFound(roleId));
            roles.Add(role);
        }

        // Assign all roles
        foreach (var role in roles)
        {
            user.AddRole(role);
        }
    }

    /// <summary>
    /// Sets (replaces) all roles assigned to a user by role IDs.
    /// This removes all existing roles and assigns only the new ones.
    /// </summary>
    public async Task SetRolesAsync(User user, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roleIds);

        var roleIdList = roleIds.ToList();

        // If empty list, just clear all roles
        if (!roleIdList.Any())
        {
            user.SetRoles(Enumerable.Empty<Role>());
            return;
        }

        // Fetch all roles at once
        var roles = new List<Role>();
        foreach (var roleId in roleIdList)
        {
            var role = await _roleRepository.GetAsync(roleId, cancellationToken: cancellationToken)
                ?? throw new DomainException(DomainErrors.Role.NotFound(roleId));
            roles.Add(role);
        }

        // Replace all roles - EF Core will automatically track the changes
        user.SetRoles(roles);
    }

    /// <summary>
    /// Updates user language preference
    /// </summary>
    public void UpdateLanguage(User user, string language)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.UpdateLanguage(language);
    }

    /// <summary>
    /// Updates user FCM token for push notifications
    /// Registers user in Firebase Auth if not already registered
    /// </summary>
    public async Task UpdateFcmTokenAsync(User user, string? fcmToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Register user in Firebase Auth if not already registered
        if (_firebaseAuthService != null && !string.IsNullOrWhiteSpace(user.Email))
        {
            try
            {
                // Check if user exists in Firebase Auth
                var firebaseUid = await _firebaseAuthService.GetFirebaseUidAsync(user.Email, cancellationToken);

                if (string.IsNullOrEmpty(firebaseUid))
                {
                    // User doesn't exist in Firebase Auth, create them
                    await _firebaseAuthService.CreateUserAsync(user.Email, cancellationToken);
                }
            }
            catch
            {
                // Continue with FCM token update even if Firebase Auth registration fails
                // Errors are logged by the FirebaseAuthService implementation
            }
        }

        // Update FCM token in user entity
        user.UpdateFcmToken(fcmToken);
    }

    /// <summary>
    /// Updates user FCM token for push notifications (synchronous version for backward compatibility)
    /// </summary>
    public void UpdateFcmToken(User user, string? fcmToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        // For synchronous calls, we'll just update the token without Firebase Auth registration
        // Firebase Auth registration should be done via UpdateFcmTokenAsync
        user.UpdateFcmToken(fcmToken);
    }
}

