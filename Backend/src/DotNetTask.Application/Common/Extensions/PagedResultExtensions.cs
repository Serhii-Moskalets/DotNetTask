using DotNetTask.Application.Common.Dtos;

namespace DotNetTask.Application.Common.Extensions;

/// <summary>
/// Provides extension methods for creating paged results.
/// </summary>
public static class PagedResultExtensions
{
    /// <summary>
    /// Converts a collection to a <see cref="PagedResultDto{TDest}"/> using a mapping function.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements.</typeparam>
    /// <typeparam name="TDest">The type of the destination elements.</typeparam>
    /// <param name="source">The source collection to map.</param>
    /// <param name="totalCount">The total number of items available in the data store.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="mapFunc">The function to map the source collection to the destination collection.</param>
    /// <returns>A <see cref="PagedResultDto{TDest}"/> containing the mapped items and pagination metadata.</returns>
    public static PagedResultDto<TDest> ToPagedResult<TSource, TDest>(
        this IReadOnlyCollection<TSource> source,
        int totalCount,
        int page,
        int pageSize,
        Func<IReadOnlyCollection<TSource>, IReadOnlyCollection<TDest>> mapFunc)
    {
        return new PagedResultDto<TDest>
        {
            Items = mapFunc(source),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
