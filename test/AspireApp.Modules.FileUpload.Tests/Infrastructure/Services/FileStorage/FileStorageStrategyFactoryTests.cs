using AspireApp.Modules.FileUpload.Domain.Enums;
using AspireApp.Modules.FileUpload.Domain.Interfaces;
using AspireApp.Modules.FileUpload.Infrastructure.Services.FileStorage;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspireApp.Modules.FileUpload.Tests.Infrastructure.Services.FileStorage;

public class FileStorageStrategyFactoryTests
{
    [Fact]
    public void GetStrategy_ShouldReturnCorrectStrategy_WhenRegistered()
    {
        // Arrange
        var mockStrategy1 = new Mock<IFileStorageStrategy>();
        mockStrategy1.Setup(x => x.StorageType).Returns(FileStorageType.FileSystem);

        var mockStrategy2 = new Mock<IFileStorageStrategy>();
        mockStrategy2.Setup(x => x.StorageType).Returns(FileStorageType.Database);

        var strategies = new List<IFileStorageStrategy> { mockStrategy1.Object, mockStrategy2.Object };
        var factory = new FileStorageStrategyFactory(strategies);

        // Act
        var result = factory.GetStrategy(FileStorageType.FileSystem);

        // Assert
        result.Should().BeSameAs(mockStrategy1.Object);
    }

    [Fact]
    public void GetStrategy_ShouldThrowException_WhenNotRegistered()
    {
        // Arrange
        var strategies = new List<IFileStorageStrategy>();
        var factory = new FileStorageStrategyFactory(strategies);

        // Act
        Action act = () => factory.GetStrategy(FileStorageType.FileSystem);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("Storage type FileSystem is not supported or not registered.");
    }

    [Fact]
    public void GetAllStrategies_ShouldReturnAllRegisteredStrategies()
    {
        // Arrange
        var mockStrategy1 = new Mock<IFileStorageStrategy>();
        mockStrategy1.Setup(x => x.StorageType).Returns(FileStorageType.FileSystem);

        var mockStrategy2 = new Mock<IFileStorageStrategy>();
        mockStrategy2.Setup(x => x.StorageType).Returns(FileStorageType.Database);

        var strategies = new List<IFileStorageStrategy> { mockStrategy1.Object, mockStrategy2.Object };
        var factory = new FileStorageStrategyFactory(strategies);

        // Act
        var result = factory.GetAllStrategies();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(mockStrategy1.Object);
        result.Should().Contain(mockStrategy2.Object);
    }
}
