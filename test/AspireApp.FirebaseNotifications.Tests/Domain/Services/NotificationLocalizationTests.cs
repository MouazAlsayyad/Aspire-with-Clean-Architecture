
using AspireApp.ApiService.Domain.Users.Entities;
using AspireApp.ApiService.Domain.Users.Interfaces;
using AspireApp.FirebaseNotifications.Domain.Services;
using FluentAssertions;
using Moq;

namespace AspireApp.FirebaseNotifications.Tests.Domain.Services;

public class NotificationLocalizationTests
{
    // Tests for static NotificationLocalization class are a bit tricky because of static state.
    // However, usually we can test the public methods.
    // Note: Localization usually depends on files on disk. For unit tests, we might not have them.
    // We should check if we can simulate it or if we should skip file-dependent tests.
    // Looking at the code: LoadResources checks current directory.
    // We can create a temporary resource file for the test.

    public class StaticTests : IDisposable
    {
        private readonly string _resourceDir;
        private readonly string _enFile;

        public StaticTests()
        {
            _resourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            if (!Directory.Exists(_resourceDir))
                Directory.CreateDirectory(_resourceDir);

            _enFile = Path.Combine(_resourceDir, "NotificationResources.json");
            File.WriteAllText(_enFile, "{\"TestKey\": \"Test Value\", \"FormatKey\": \"Hello {0}\"}");

            // Force reload
            NotificationLocalization.ReloadResources();
        }

        public void Dispose()
        {
            if (File.Exists(_enFile)) File.Delete(_enFile);
            // Directory cleanup might be risky if parallell tests run, but okay for now
        }

        [Fact]
        public void GetString_ShouldReturnLocalizedValue_WhenKeyExists()
        {
            var result = NotificationLocalization.GetString("TestKey", "en");
            result.Should().Be("Test Value");
        }

        [Fact]
        public void GetString_ShouldReturnKey_WhenKeyDoesNotExist()
        {
            var result = NotificationLocalization.GetString("NonExistentKey", "en");
            result.Should().Be("NonExistentKey");
        }

        [Fact]
        public void GetString_ShouldFormatString_WhenArgsProvided()
        {
            var result = NotificationLocalization.GetString("FormatKey", "en", "World");
            result.Should().Be("Hello World");
        }
    }

    public class ServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly NotificationLocalizationService _service;

        public ServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _service = new NotificationLocalizationService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUserLanguageAsync_ShouldReturnUserLanguage_WhenUserExists()
        {
            // Arrange
            var user = new User(
                "test@example.com",
                "testuser",
                new ApiService.Domain.ValueObjects.PasswordHash("hash", "salt"),
                "First",
                "Last");

            user.UpdateLanguage("ar");
            var userId = user.Id;

            _userRepositoryMock.Setup(x => x.GetAsync(userId, default, default))
                .ReturnsAsync(user);

            // Act
            var lang = await _service.GetUserLanguageAsync(userId);

            // Assert
            lang.Should().Be("ar");
        }

        [Fact]
        public async Task GetUserLanguageAsync_ShouldReturnEn_WhenUserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>(), default, default))
                .ReturnsAsync((User?)null);

            // Act
            var lang = await _service.GetUserLanguageAsync(Guid.NewGuid());

            // Assert
            lang.Should().Be("en");
        }
    }
}
