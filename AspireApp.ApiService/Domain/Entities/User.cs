using AspireApp.ApiService.Domain.ValueObjects;

namespace AspireApp.ApiService.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; } = false;
    public bool IsActive { get; private set; } = true;

    // Navigation property for many-to-many relationship
    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // Private constructor for EF Core
    private User() { }

    public User(string email, string userName, PasswordHash passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("UserName cannot be empty", nameof(userName));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be empty", nameof(lastName));

        Email = email.ToLowerInvariant();
        UserName = userName;
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdatePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        SetLastModificationTime();
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        SetLastModificationTime();
    }

    public void Activate()
    {
        IsActive = true;
        SetLastModificationTime();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetLastModificationTime();
    }

    public void AddRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return; // Role already assigned

        var userRole = new UserRole(Id, role.Id);
        _userRoles.Add(userRole);
        SetLastModificationTime();
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            SetLastModificationTime();
        }
    }

    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur => ur.Role != null && ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permissionName)
    {
        return _userRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Any(rp => rp.Permission != null && 
                rp.Permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<string> GetAllPermissions()
    {
        return _userRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .Distinct();
    }
}

