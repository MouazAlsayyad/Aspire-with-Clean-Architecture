using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.ApiService.Domain.ValueObjects;
using AspireApp.FirebaseNotifications.Domain.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.FirebaseNotifications.Tests.Domain.Services;

public class NotificationLocalizationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly NotificationLocalizationService _service;

    public NotificationLocalizationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new NotificationLocalizationService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetUserLanguageAsync_ShouldReturnUserLanguage_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateLanguage("ar");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, default))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserLanguageAsync(userId);

        // Assert
        result.Should().Be("ar");
        _userRepositoryMock.Verify(x => x.GetAsync(userId, default), Times.Once);
    }

    [Fact]
    public async Task GetUserLanguageAsync_ShouldReturnEnglish_WhenUserLanguageIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        // Language is null by default

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, default))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserLanguageAsync(userId);

        // Assert
        result.Should().Be("en");
    }

    [Fact]
    public async Task GetUserLanguageAsync_ShouldReturnEnglish_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, default))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserLanguageAsync(userId);

        // Assert
        result.Should().Be("en");
    }

    [Fact]
    public async Task GetLocalizedContentAsync_ShouldUseUserLanguage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateLanguage("ar");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, default))
            .ReturnsAsync(user);

        // Initialize the localization system
        NotificationLocalization.Initialize();

        // Act
        var result = await _service.GetLocalizedContentAsync(userId, "Test_Title", "Test_Body");

        // Assert
        result.Should().NotBeNull();
        result.Language.Should().Be("ar");
        _userRepositoryMock.Verify(x => x.GetAsync(userId, default), Times.Once);
    }

    [Fact]
    public async Task GetLocalizedContentAsync_ShouldDefaultToEnglish_WhenUserLanguageIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, default))
            .ReturnsAsync(user);

        // Initialize the localization system
        NotificationLocalization.Initialize();

        // Act
        var result = await _service.GetLocalizedContentAsync(userId, "Test_Title", "Test_Body");

        // Assert
        result.Should().NotBeNull();
        result.Language.Should().Be("en");
    }

    [Fact]
    public async Task GetLocalizedContentAsync_ShouldAcceptFormatArgs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("test@example.com", "TestUser", new PasswordHash("hashed_password", "test_salt"), "Test", "User");
        user.UpdateLanguage("en");

        _userRepositoryMock.Setup(x => x.GetAsync(userId, false, default))
            .ReturnsAsync(user);

        // Initialize the localization system
        NotificationLocalization.Initialize();

        // Act
        var result = await _service.GetLocalizedContentAsync(userId, "Test_Title", "Test_Body", "arg1", "arg2");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().NotBeNull();
        result.Body.Should().NotBeNull();
    }

    [Fact]
    public void ReloadResources_ShouldNotThrow()
    {
        // Act
        var act = () => _service.ReloadResources();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ReloadResources_ShouldReinitializeLocalizationSystem()
    {
        // Arrange
        NotificationLocalization.Initialize();
        var languagesBefore = NotificationLocalization.GetAvailableLanguages();

        // Act
        _service.ReloadResources();
        var languagesAfter = NotificationLocalization.GetAvailableLanguages();

        // Assert
        languagesAfter.Should().NotBeNull();
        // Languages should still be available after reload
    }

    [Fact]
    public void Service_ShouldImplementINotificationLocalizationService()
    {
        // Assert
        _service.Should().BeAssignableTo<AspireApp.FirebaseNotifications.Domain.Interfaces.INotificationLocalizationService>();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenValidDependencies()
    {
        // Act & Assert
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserLanguageAsync_ShouldHandleMultipleLanguages()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var user1 = new User("user1@example.com", "User1", new PasswordHash("hashed_password", "test_salt"), "User", "One");
        user1.UpdateLanguage("en");

        var user2 = new User("user2@example.com", "User2", new PasswordHash("hashed_password", "test_salt"), "User", "Two");
        user2.UpdateLanguage("ar");

        _userRepositoryMock.Setup(x => x.GetAsync(userId1, false, default))
            .ReturnsAsync(user1);
        _userRepositoryMock.Setup(x => x.GetAsync(userId2, false, default))
            .ReturnsAsync(user2);

        // Act
        var result1 = await _service.GetUserLanguageAsync(userId1);
        var result2 = await _service.GetUserLanguageAsync(userId2);

        // Assert
        result1.Should().Be("en");
        result2.Should().Be("ar");
    }
}

