using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

namespace DotNetTask.Application.Abstractions.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="TagEntity"/>.
/// Provides methods for querying, creating, updating, and deleting tags.
/// </summary>
public interface ITagRepository : IRepository<TagEntity>
{
    /// <summary>
    /// Retrieves all tags owned by a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of tags per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="TagEntity"/>.</returns>
    Task<(IReadOnlyCollection<TagEntity> Items, int TotalCount)> GetTagsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tag entity by its identifier for a specific user.
    /// </summary>
    /// <param name="tagId">The unique identifier of the tag.</param>
    /// <param name="userId">The unique identifier of the user who owns the task list.</param>
    /// <param name="asNoTracking">
    /// If <c>true</c>, the query will not track changes in the retrieved entity,
    /// which can improve performance for read-only operations.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// The tag entity with the specified ID for the given user, or <c>null</c> if not found.
    /// </returns>
    Task<TagEntity?> GetTagByIdForUserAsync(
        Guid tagId,
        Guid userId,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a tag with the specified name exists for a specific user.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if a tag with the specified name exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByNameAsync(TagName name, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a specific user is the owner of a tag.
    /// </summary>
    /// <param name="tagId">The identifier of the tag.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the user is the owner of the tag; otherwise, <c>false</c>.</returns>
    Task<bool> IsTagOwnerAsync(Guid tagId, Guid userId, CancellationToken cancellationToken = default);
}
