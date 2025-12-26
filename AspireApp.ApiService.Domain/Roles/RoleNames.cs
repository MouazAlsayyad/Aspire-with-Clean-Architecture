namespace AspireApp.ApiService.Domain.Roles;

/// <summary>
/// Static class containing all role names.
/// Use these constants instead of magic strings throughout the application.
/// </summary>
public static class RoleNames
{
    /// <summary>
    /// Administrator role with full access to all resources
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Manager role with elevated permissions
    /// </summary>
    public const string Manager = "Manager";

    /// <summary>
    /// Standard user role with basic permissions
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Gets all roles in the system
    /// </summary>
    public static string[] GetAll()
    {
        return [Admin, Manager, User];
    }
}

