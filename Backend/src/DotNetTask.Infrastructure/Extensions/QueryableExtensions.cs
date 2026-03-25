namespace DotNetTask.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IQueryable{T}"/> to simplify data manipulation.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies pagination logic to the specified query.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the query.</typeparam>
    /// <param name="query">The source <see cref="IQueryable{T}"/> to apply pagination to.</param>
    /// <param name="page">The page number (1-based index). If less than 1, it defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. If less than 1, it defaults to 10.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> with <see cref="Queryable.Skip{TSource}(IQueryable{TSource}, int)"/>
    /// and <see cref="Queryable.Take{TSource}(IQueryable{TSource}, int)"/> applied.
    /// </returns>
    /// <remarks>
    /// This method ensures that the pagination parameters are within valid ranges
    /// to prevent database-level errors when negative values are provided.
    /// </remarks>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}
