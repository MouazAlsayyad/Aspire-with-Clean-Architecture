using AspireApp.Domain.Shared.Common;

namespace AspireApp.Domain.Shared.Tests.Common;

public class ErrorsTests
{
    [Fact]
    public void UserNotFound_WithId_ShouldReturnNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var error = DomainErrors.User.NotFound(userId);

        // Assert
        Assert.Equal("User.NotFound", error.Code);
        Assert.Contains(userId.ToString(), error.Message);
        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public void UserNotFound_WithEmail_ShouldReturnNotFoundError()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var error = DomainErrors.User.NotFound(email);

        // Assert
        Assert.Equal("User.NotFound", error.Code);
        Assert.Contains(email, error.Message);
        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public void UserEmailAlreadyExists_ShouldReturnConflictError()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var error = DomainErrors.User.EmailAlreadyExists(email);

        // Assert
        Assert.Equal("User.EmailAlreadyExists", error.Code);
        Assert.Contains(email, error.Message);
        Assert.Equal(ErrorType.Conflict, error.Type);
    }

    [Fact]
    public void UserInvalidCredentials_ShouldReturnUnauthorizedError()
    {
        // Act
        var error = DomainErrors.User.InvalidCredentials();

        // Assert
        Assert.Equal("User.InvalidCredentials", error.Code);
        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void UserAccountDeactivated_ShouldReturnForbiddenError()
    {
        // Act
        var error = DomainErrors.User.AccountDeactivated();

        // Assert
        Assert.Equal("User.AccountDeactivated", error.Code);
        Assert.Equal(ErrorType.Forbidden, error.Type);
    }

    [Fact]
    public void RoleNotFound_WithId_ShouldReturnNotFoundError()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        // Act
        var error = DomainErrors.Role.NotFound(roleId);

        // Assert
        Assert.Equal("Role.NotFound", error.Code);
        Assert.Contains(roleId.ToString(), error.Message);
        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public void RoleCannotDeleteAssignedRole_ShouldReturnConflictError()
    {
        // Arrange
        var roleName = "Admin";

        // Act
        var error = DomainErrors.Role.CannotDeleteAssignedRole(roleName);

        // Assert
        Assert.Equal("Role.CannotDeleteAssignedRole", error.Code);
        Assert.Contains(roleName, error.Message);
        Assert.Equal(ErrorType.Conflict, error.Type);
    }

    [Fact]
    public void PermissionNotFound_WithName_ShouldReturnNotFoundError()
    {
        // Arrange
        var permissionName = "User.Read";

        // Act
        var error = DomainErrors.Permission.NotFound(permissionName);

        // Assert
        Assert.Equal("Permission.NotFound", error.Code);
        Assert.Contains(permissionName, error.Message);
        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public void PermissionCannotDeleteAssignedPermission_ShouldReturnConflictError()
    {
        // Arrange
        var permissionName = "User.Delete";

        // Act
        var error = DomainErrors.Permission.CannotDeleteAssignedPermission(permissionName);

        // Assert
        Assert.Equal("Permission.CannotDeleteAssignedPermission", error.Code);
        Assert.Contains(permissionName, error.Message);
        Assert.Equal(ErrorType.Conflict, error.Type);
    }

    [Fact]
    public void RefreshTokenInvalid_ShouldReturnUnauthorizedError()
    {
        // Act
        var error = DomainErrors.RefreshToken.Invalid();

        // Assert
        Assert.Equal("RefreshToken.Invalid", error.Code);
        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void RefreshTokenRevoked_ShouldReturnUnauthorizedError()
    {
        // Act
        var error = DomainErrors.RefreshToken.Revoked();

        // Assert
        Assert.Equal("RefreshToken.Revoked", error.Code);
        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void RefreshTokenExpired_ShouldReturnUnauthorizedError()
    {
        // Act
        var error = DomainErrors.RefreshToken.Expired();

        // Assert
        Assert.Equal("RefreshToken.Expired", error.Code);
        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void GeneralUnauthorized_ShouldReturnUnauthorizedError()
    {
        // Act
        var error = DomainErrors.General.Unauthorized();

        // Assert
        Assert.Equal("General.Unauthorized", error.Code);
        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void GeneralServerError_WithMessage_ShouldReturnFailureError()
    {
        // Arrange
        var message = "Database connection failed";

        // Act
        var error = DomainErrors.General.ServerError(message);

        // Assert
        Assert.Equal("General.ServerError", error.Code);
        Assert.Contains(message, error.Message);
        Assert.Equal(ErrorType.Failure, error.Type);
    }
}

