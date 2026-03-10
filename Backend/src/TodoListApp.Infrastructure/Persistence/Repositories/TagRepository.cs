using Microsoft.EntityFrameworkCore;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;
using TodoListApp.Infrastructure.Extensions;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;

namespace TodoListApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="TagEntity"/> objects in the database.
/// Provides methods to check existence, retrieve, paginate, and verify ownership of tags.
/// Initializes a new instance of the repository with the specified database context.
/// </summary>
public class TagRepository(TodoListAppDbContext context)
    : BaseRepository<TagEntity>(context), ITagRepository
{
    /// <summary>
    /// Checks if a tag with the specified name exists for a specific user.
    /// </summary>
    /// <param name="name">The name of the tag to check.</param>
    /// <param name="userId">The ID of the user who owns the tag.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if a tag with the specified name exists for the user; otherwise, <c>false</c>.</returns>
    public Task<bool> ExistsByNameAsync(TagName name, Guid userId, CancellationToken cancellationToken = default)
        => this.DbSet.AsNoTracking().AnyAsync(x => x.UserId == userId && x.Name == name, cancellationToken);

    /// <summary>
    /// Retrieves all tags associated with a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of tags per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of tags for the specified user.</returns>
    public async Task<(IReadOnlyCollection<TagEntity> Items, int TotalCount)> GetTagsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var tagsQuery = this.DbSet.AsNoTracking()
            .Where(x => x.UserId == userId);

        var totalCount = await tagsQuery.CountAsync(cancellationToken);

        var items = await tagsQuery
            .OrderBy(x => x.CreatedDate)
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

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
    public async Task<TagEntity?> GetTagByIdForUserAsync(Guid tagId, Guid userId, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TagEntity> query = this.DbSet;
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(x => x.Id == tagId && x.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Determines whether a specific user is the owner of a tag.
    /// </summary>
    /// <param name="tagId">The ID of the tag.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the user is the owner of the tag; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsTagOwnerAsync(Guid tagId, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking().AnyAsync(x => x.Id == tagId && x.UserId == userId, cancellationToken);
}
