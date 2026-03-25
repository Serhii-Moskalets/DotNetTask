namespace DotNetTask.Application.Common.Dtos;

/// <summary>
/// Represents a paginated collection of items with metadata.
/// </summary>
/// <typeparam name="T">The type of the items in the collection.</typeparam>
public class PagedResultDto<T>
{
    /// <summary>
    /// Gets the items for the current page.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the total number of items matching the filter.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; init; }
}
