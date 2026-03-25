using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;
using DotNetTask.Infrastructure.Extensions;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;

using Microsoft.EntityFrameworkCore;

namespace DotNetTask.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository responsible for data access operations related to <see cref="TaskListEntity"/>.
/// Provides methods for querying task lists by user, checking ownership,
/// pagination, and validating uniqueness constraints.
/// </summary>
public class TaskListRepository(DotNetTaskDbContext context)
    : BaseRepository<TaskListEntity>(context), ITaskListRepository
{
    /// <summary>
    /// Determines whether a task list with the specified title already exists for a given user.
    /// </summary>
    /// <param name="title">The title of the task list to check.</param>
    /// <param name="userId">The identifier of the user who owns the task lists.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>
    /// <c>true</c> if a task list with the specified title exists for the user;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// /// <remarks>
    /// Uses `EF.Functions.Like` for real databases, and case-insensitive comparison for InMemory provider.
    /// </remarks>
    public async Task<bool> ExistsByTitleAsync(TaskListTitle title, Guid userId, CancellationToken cancellationToken = default)
    {
        return await this.DbSet.AsNoTracking()
            .AnyAsync(x => x.Title == title && x.OwnerId == userId, cancellationToken);
    }

    /// <summary>
    /// Retrieves all task lists owned by the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user who owns the task lists.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>
    /// A read-only collection of task list instances owned by the user.
    /// </returns>
    public async Task<(IReadOnlyCollection<TaskListEntity> Items, int TotalCount)> GetTaskListsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TaskListEntity> taskListsQuery = this.DbSet.AsNoTracking()
            .Where(x => x.OwnerId == userId);

        int totalCount = await taskListsQuery.CountAsync(cancellationToken);

        List<TaskListEntity> items = await taskListsQuery
            .OrderBy(x => x.CreatedDate)
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Retrieves a task list entity by its identifier for a specific user.
    /// Includes the Tag and Comments (with User) related entities.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list.</param>
    /// <param name="userId">The unique identifier of the user who owns the task list.</param>
    /// <param name="asNoTracking">
    /// If <c>true</c>, the query will not track changes in the retrieved entity,
    /// which can improve performance for read-only operations.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// The task list entity with the specified ID for the given user, or <c>null</c> if not found.
    /// </returns>
    public async Task<TaskListEntity?> GetTaskListByIdForUserAsync(Guid taskListId, Guid userId, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TaskListEntity> query = this.DbSet;
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(x => x.Id == taskListId && x.OwnerId == userId, cancellationToken);
    }
}
