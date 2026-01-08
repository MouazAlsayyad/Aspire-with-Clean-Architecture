using AspireApp.Domain.Shared.Common;

namespace AspireApp.Domain.Shared.Tests.Common;

public class PaginationInfoTests
{
    [Fact]
    public void Constructor_WithNoParameters_ShouldCreateEmptyPaginationInfo()
    {
        // Act
        var paginationInfo = new PaginationInfo();

        // Assert
        Assert.Equal(0, paginationInfo.TotalCount);
        Assert.Equal(0, paginationInfo.PageNumber);
        Assert.Equal(0, paginationInfo.PageSize);
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetProperties()
    {
        // Arrange
        long totalCount = 100;
        int pageNumber = 2;
        int pageSize = 10;

        // Act
        var paginationInfo = new PaginationInfo(totalCount, pageNumber, pageSize);

        // Assert
        Assert.Equal(totalCount, paginationInfo.TotalCount);
        Assert.Equal(pageNumber, paginationInfo.PageNumber);
        Assert.Equal(pageSize, paginationInfo.PageSize);
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(100, 1, 10);

        // Act
        var totalPages = paginationInfo.TotalPages;

        // Assert
        Assert.Equal(10, totalPages);
    }

    [Fact]
    public void TotalPages_WithPartialPage_ShouldRoundUp()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(95, 1, 10);

        // Act
        var totalPages = paginationInfo.TotalPages;

        // Assert
        Assert.Equal(10, totalPages); // 95/10 = 9.5, rounds up to 10
    }

    [Fact]
    public void TotalPages_WithZeroPageSize_ShouldReturnZero()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(100, 1, 0);

        // Act
        var totalPages = paginationInfo.TotalPages;

        // Assert
        Assert.Equal(0, totalPages);
    }

    [Fact]
    public void HasPreviousPage_OnFirstPage_ShouldBeFalse()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(100, 1, 10);

        // Act
        var hasPreviousPage = paginationInfo.HasPreviousPage;

        // Assert
        Assert.False(hasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_OnSecondPage_ShouldBeTrue()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(100, 2, 10);

        // Act
        var hasPreviousPage = paginationInfo.HasPreviousPage;

        // Assert
        Assert.True(hasPreviousPage);
    }

    [Fact]
    public void HasNextPage_OnLastPage_ShouldBeFalse()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(100, 10, 10);

        // Act
        var hasNextPage = paginationInfo.HasNextPage;

        // Assert
        Assert.False(hasNextPage);
    }

    [Fact]
    public void HasNextPage_OnFirstPage_ShouldBeTrue()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(100, 1, 10);

        // Act
        var hasNextPage = paginationInfo.HasNextPage;

        // Assert
        Assert.True(hasNextPage);
    }

    [Fact]
    public void HasNextPage_OnMiddlePage_ShouldBeTrue()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(100, 5, 10);

        // Act
        var hasNextPage = paginationInfo.HasNextPage;

        // Assert
        Assert.True(hasNextPage);
    }

    [Fact]
    public void PaginationInfo_WithSingleItem_ShouldCalculateCorrectly()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(1, 1, 10);

        // Act & Assert
        Assert.Equal(1, paginationInfo.TotalPages);
        Assert.False(paginationInfo.HasPreviousPage);
        Assert.False(paginationInfo.HasNextPage);
    }

    [Fact]
    public void PaginationInfo_WithNoItems_ShouldHandleGracefully()
    {
        // Arrange
        var paginationInfo = new PaginationInfo(0, 1, 10);

        // Act & Assert
        Assert.Equal(0, paginationInfo.TotalPages);
        Assert.False(paginationInfo.HasPreviousPage);
        Assert.False(paginationInfo.HasNextPage);
    }
}

