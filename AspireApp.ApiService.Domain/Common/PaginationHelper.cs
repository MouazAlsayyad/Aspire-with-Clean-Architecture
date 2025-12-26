namespace AspireApp.ApiService.Domain.Common;

/// <summary>
/// Static helper class for pagination calculations
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Validates and normalizes the pagination parameters
    /// </summary>
    /// <param name="pageNumber">Page number (will be normalized if invalid)</param>
    /// <param name="pageSize">Page size (will be normalized if invalid)</param>
    public static void Normalize(ref int pageNumber, ref int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100; // Max page size limit
    }

    /// <summary>
    /// Calculates the number of items to skip
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Number of items to skip</returns>
    public static int Skip(int pageNumber, int pageSize)
    {
        return (pageNumber - 1) * pageSize;
    }

    /// <summary>
    /// Gets the number of items to take (same as pageSize)
    /// </summary>
    /// <param name="pageSize">Page size</param>
    /// <returns>Number of items to take</returns>
    public static int Take(int pageSize)
    {
        return pageSize;
    }
}

/// <summary>
/// Sort direction enumeration
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending
}

