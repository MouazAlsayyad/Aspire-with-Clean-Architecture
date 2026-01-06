using AspireApp.ApiService.Infrastructure.Extensions;
using AspireApp.Domain.Shared.Entities;
using AspireApp.Domain.Shared.Interfaces;
using AspireApp.ApiService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration; // Add this
using NSubstitute;

namespace AspireApp.ApiService.Infrastructure.Tests.Extensions;

// Dummy entities for testing registration scanning if needed, 
// but using existing project types is better to test actual logic.

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRepositories_ShouldRegisterGenericRepositories()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRepositories();
        var provider = services.BuildServiceProvider();

        // Assert
        // We check if IRepository<Role> is registered. Role is in AspireApp.ApiService.Infrastructure usually or Domain.
        // Based on file structure, Role is likely in AspireApp.ApiService.Domain or Infrastructure.
        // Let's assume Role exists and Repository<Role> should be there.
        // Or checking generic open type registration is harder with GetService.

        // We can check service collection directly
        services.Should().Contain(d => d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(IRepository<>));
    }

    [Fact]
    public void AddUnitOfWork_ShouldRegisterIUnitOfWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddUnitOfWork();

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(IUnitOfWork) && d.ImplementationType == typeof(UnitOfWork));
    }

    [Fact]
    public void AddHttpContextAccessorService_ShouldRegisterAccessor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHttpContextAccessorService();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Should().NotBeNull();
    }

    [Fact]
    public void AddDomainEventDispatcher_ShouldRegisterDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDomainEventDispatcher();

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(IDomainEventDispatcher) && d.ImplementationType == typeof(AspireApp.ApiService.Infrastructure.DomainEvents.DomainEventDispatcher));
    }
}
