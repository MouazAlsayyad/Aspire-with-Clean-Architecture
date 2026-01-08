using AspireApp.ApiService.Domain.Services;
using AspireApp.Domain.Shared.Interfaces;

namespace AspireApp.ApiService.Domain.Tests.Services;

public class DomainServiceTests
{
    [Fact]
    public void DomainService_ShouldImplementIDomainService()
    {
        // Arrange
        var service = new TestDomainService();

        // Assert
        Assert.IsAssignableFrom<IDomainService>(service);
    }

    [Fact]
    public void DomainService_CanBeInstantiated()
    {
        // Act
        var service = new TestDomainService();

        // Assert
        Assert.NotNull(service);
    }

    // Test implementation class
    private class TestDomainService : DomainService
    {
    }
}

