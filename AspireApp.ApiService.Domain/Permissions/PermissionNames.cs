namespace AspireApp.ApiService.Domain.Permissions;

/// <summary>
/// Represents a permission definition with metadata
/// </summary>
public record PermissionDefinition(
    string Name,
    string Description,
    string Resource,
    string Action
);

/// <summary>
/// Static class containing all permission names.
/// Use these constants instead of magic strings throughout the application.
/// </summary>
public static class PermissionNames
{
    /// <summary>
    /// Weather module permissions
    /// </summary>
    public static class Weather
    {
        public const string Read = "Weather.Read";
        public const string Write = "Weather.Write";
        public const string Delete = "Weather.Delete";

        /// <summary>
        /// Gets all weather permissions
        /// </summary>
        public static string[] GetAll()
        {
            return [Read, Write, Delete];
        }
    }

    /// <summary>
    /// User module permissions
    /// </summary>
    public static class User
    {
        public const string Read = "User.Read";
        public const string Write = "User.Write";
        public const string Delete = "User.Delete";

        /// <summary>
        /// Gets all user permissions
        /// </summary>
        public static string[] GetAll()
        {
            return [Read, Write, Delete];
        }
    }

    /// <summary>
    /// Role module permissions
    /// </summary>
    public static class Role
    {
        public const string Read = "Role.Read";
        public const string Write = "Role.Write";
        public const string Delete = "Role.Delete";

        /// <summary>
        /// Gets all role permissions
        /// </summary>
        public static string[] GetAll()
        {
            return [Read, Write, Delete];
        }
    }

    /// <summary>
    /// Permission module permissions
    /// </summary>
    public static class Permission
    {
        public const string Read = "Permission.Read";
        public const string Write = "Permission.Write";
        public const string Delete = "Permission.Delete";

        /// <summary>
        /// Gets all permission module permissions
        /// </summary>
        public static string[] GetAll()
        {
            return [Read, Write, Delete];
        }
    }

    /// <summary>
    /// ActivityLog module permissions
    /// </summary>
    public static class ActivityLog
    {
        public const string Read = "ActivityLog.Read";

        /// <summary>
        /// Gets all activity log permissions
        /// </summary>
        public static string[] GetAll()
        {
            return [Read];
        }
    }

    /// <summary>
    /// FileUpload module permissions
    /// </summary>
    public static class FileUpload
    {
        public const string Read = "FileUpload.Read";
        public const string Write = "FileUpload.Write";
        public const string Delete = "FileUpload.Delete";

        /// <summary>
        /// Gets all file upload permissions
        /// </summary>
        public static string[] GetAll()
        {
            return [Read, Write, Delete];
        }
    }

    /// <summary>
    /// Gets all permissions in the system
    /// </summary>
    public static string[] GetAll()
    {
        return [..Weather.GetAll()
            .Concat(User.GetAll())
            .Concat(Role.GetAll())
            .Concat(Permission.GetAll())
            .Concat(ActivityLog.GetAll())
            .Concat(FileUpload.GetAll())];
    }

    /// <summary>
    /// Gets all read permissions
    /// </summary>
    public static string[] GetAllRead()
    {
        return
        [
            Weather.Read,
            User.Read,
            Role.Read,
            Permission.Read,
            ActivityLog.Read,
            FileUpload.Read
        ];
    }

    /// <summary>
    /// Gets all write permissions
    /// </summary>
    public static string[] GetAllWrite()
    {
        return
        [

            Weather.Write,
            User.Write,
            Role.Write,
            Permission.Write,
            FileUpload.Write
        ];
    }

    /// <summary>
    /// Gets all delete permissions
    /// </summary>
    public static string[] GetAllDelete()
    {
        return
        [
            Weather.Delete,
            User.Delete,
            Role.Delete,
            Permission.Delete,
            FileUpload.Delete
        ];
    }

    /// <summary>
    /// Gets all permission definitions with metadata for seeding
    /// </summary>
    public static PermissionDefinition[] GetAllDefinitions()
    {
        return
        [
            // Weather permissions
            new PermissionDefinition(Weather.Read, "Read weather forecast", "Weather", "Read"),
            new PermissionDefinition(Weather.Write, "Write weather forecast", "Weather", "Write"),
            new PermissionDefinition(Weather.Delete, "Delete weather forecast", "Weather", "Delete"),
            
            // User permissions
            new PermissionDefinition(User.Read, "Read user information", "User", "Read"),
            new PermissionDefinition(User.Write, "Create or update users", "User", "Write"),
            new PermissionDefinition(User.Delete, "Delete users", "User", "Delete"),
            
            // Role permissions
            new PermissionDefinition(Role.Read, "Read roles", "Role", "Read"),
            new PermissionDefinition(Role.Write, "Create or update roles", "Role", "Write"),
            new PermissionDefinition(Role.Delete, "Delete roles", "Role", "Delete"),
            
            // Permission module permissions
            new PermissionDefinition(Permission.Read, "Read permissions", "Permission", "Read"),
            new PermissionDefinition(Permission.Write, "Create or update permissions", "Permission", "Write"),
            new PermissionDefinition(Permission.Delete, "Delete permissions", "Permission", "Delete"),
            
            // ActivityLog permissions
            new PermissionDefinition(ActivityLog.Read, "Read activity logs", "ActivityLog", "Read"),
            
            // FileUpload permissions
            new PermissionDefinition(FileUpload.Read, "Read file uploads", "FileUpload", "Read"),
            new PermissionDefinition(FileUpload.Write, "Upload files", "FileUpload", "Write"),
            new PermissionDefinition(FileUpload.Delete, "Delete file uploads", "FileUpload", "Delete")
        ];
    }
}
