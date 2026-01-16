using AspireApp.ApiService.Infrastructure.Helpers;
using FluentAssertions;

namespace AspireApp.ApiService.Infrastructure.Tests.Helpers;

public class SensitiveDataFilterTests
{
    [Fact]
    public void FilterJson_ShouldMaskSensitiveFields()
    {
        // Arrange
        var json = "{\"password\":\"123456\", \"username\":\"user\", \"nested\": {\"secret\": \"hidden\"}}";

        // Act
        var result = SensitiveDataFilter.FilterJson(json);

        // Assert
        result.Should().Contain("***REDACTED***");
        result.Should().NotContain("123456");
        result.Should().NotContain("hidden");
        result.Should().Contain("user");
    }

    [Fact]
    public void FilterString_ShouldMaskQueryStringStyle()
    {
        // Arrange
        var input = "password=123&username=user&apiKey=abc";

        // Act
        var result = SensitiveDataFilter.FilterString(input);

        // Assert
        result.Should().Contain("password=***REDACTED***");
        result.Should().Contain("apiKey=***REDACTED***");
        result.Should().Contain("username=user");
    }

    [Fact]
    public void FilterDictionary_ShouldMaskSensitiveKeys()
    {
        // Arrange
        var dict = new Dictionary<string, object>
        {
            { "password", "123" },
            { "email", "test@test.com" }
        };

        // Act
        var result = SensitiveDataFilter.FilterDictionary(dict);

        // Assert
        result["password"].Should().Be("***REDACTED***");
        result["email"].Should().Be("test@test.com");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("Password")]
    [InlineData("api_key")]
    [InlineData("creditCard")]
    public void IsSensitiveField_ShouldReturnTrue_ForKnownSensitiveFields(string field)
    {
        SensitiveDataFilter.IsSensitiveField(field).Should().BeTrue();
    }
}
