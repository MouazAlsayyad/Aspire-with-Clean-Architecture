using AspireApp.Domain.Shared.Common;

namespace AspireApp.Domain.Shared.Tests.Common;

public class PaginationHelperTests
{
    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-5, 0, 1, 10)]
    [InlineData(1, 200, 1, 100)]
    [InlineData(5, 50, 5, 50)]
    public void Normalize_ShouldEnsureValidRanges(int inputPage, int inputSize, int expectedPage, int expectedSize)
    {
        // Act
        int page = inputPage;
        int size = inputSize;
        PaginationHelper.Normalize(ref page, ref size);

        // Assert
        Assert.Equal(expectedPage, page);
        Assert.Equal(expectedSize, size);
    }

    [Fact]
    public void Skip_ShouldCalculateCorrectly()
    {
        // Act & Assert
        Assert.Equal(0, PaginationHelper.Skip(1, 10));
        Assert.Equal(20, PaginationHelper.Skip(3, 10));
    }
}
