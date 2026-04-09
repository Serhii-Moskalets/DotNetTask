using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Infrastructure.Extensions;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;

using Microsoft.EntityFrameworkCore;

using TinyResult;

namespace DotNetTask.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository responsible for managing <see cref="TaskEntity"/> instances.
/// Provides methods for querying, paginating, filtering, and managing tasks,
/// including overdue tasks and task tags.
/// </summary>
public class TaskRepository(DotNetTaskDbContext context)
   : BaseRepository<TaskEntity>(context), ITaskRepository
{
    /// <summary>
    /// Counts the number of overdue tasks for a specific user within a specific task list.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="taskListId">The identifier of the task list.</param>
    /// <param name="now">The current date and time used to determine overdue tasks.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of overdue tasks.</returns>
    public async Task<int> CountOverdueTasksAsync(
        Guid userId,
        Guid taskListId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return await this.DbSet
                .Where(x => x.OwnerId == userId && x.TaskListId == taskListId && x.DueDate < now)
                .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a task entity by its identifier for a specific user.
    /// Includes the Tag and Comments (with User) related entities.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user who owns the task.</param>
    /// <param name="asNoTracking">
    /// If <c>true</c>, the query will not track changes in the retrieved entity,
    /// which can improve performance for read-only operations.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// The task entity with the specified ID for the given user, or <c>null</c> if not found.
    /// </returns>
    public async Task<TaskEntity?> GetTaskByIdForUserAsync(
        Guid taskId,
        Guid userId,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TaskEntity> query = this.DbSet;
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .Include(x => x.Tag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(x => x.Id == taskId && x.OwnerId == userId, cancellationToken);
    }

    /// <summary>
    /// Retrieves tasks for a specific user and To-Do list with optional filtering and sorting.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="todoListId">The To-Do list identifier.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="statuses">Optional collection of task statuses to filter by.</param>
    /// <param name="dueBefore">Optional upper bound for the task due date.</param>
    /// <param name="dueAfter">Optional lower bound for the task due date.</param>
    /// <param name="sortBy">Optional field name to sort by.</param>
    /// <param name="ascending">Indicates whether sorting should be ascending.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only collection of tasks.</returns>
    public async Task<(IReadOnlyCollection<TaskEntity> items, int TotalCount)> GetTasksAsync(
        Guid userId,
        Guid todoListId,
        int page = 1,
        int pageSize = 10,
        IReadOnlyCollection<StatusTask>? statuses = null,
        DateTime? dueBefore = null,
        DateTime? dueAfter = null,
        TaskSortBy? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TaskEntity> tasksQuery = this.DbSet.AsNoTracking()
            .Where(x => x.OwnerId == userId && x.TaskListId == todoListId);

        if (statuses is { Count: > 0 })
        {
            tasksQuery = tasksQuery.Where(x => statuses.Contains(x.Status));
        }

        if (dueAfter.HasValue)
        {
            tasksQuery = tasksQuery.Where(x => x.DueDate >= dueAfter);
        }

        if (dueBefore.HasValue)
        {
            tasksQuery = tasksQuery.Where(x => x.DueDate <= dueBefore);
        }

        int totalCount = await tasksQuery.CountAsync(cancellationToken);

        tasksQuery = sortBy switch
        {
            TaskSortBy.CreatedDate => ascending
                ? tasksQuery.OrderBy(t => t.CreatedDate)
                : tasksQuery.OrderByDescending(t => t.CreatedDate),

            TaskSortBy.DueDate => ascending
                ? tasksQuery.OrderBy(t => t.DueDate)
                : tasksQuery.OrderByDescending(t => t.DueDate),

            TaskSortBy.Title => ascending
                ? tasksQuery.OrderBy(t => t.Title)
                : tasksQuery.OrderByDescending(t => t.Title),

            TaskSortBy.Status => ascending
                ? tasksQuery.OrderBy(t => t.Status)
                : tasksQuery.OrderByDescending(t => t.Status),

            _ => tasksQuery.OrderByDescending(t => t.CreatedDate),
        };

        List<TaskEntity> items = await tasksQuery
                .Include(x => x.Tag)
                .ApplyPagination(page, pageSize)
                .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Searches tasks by title for a specific user.
    /// Includes Tag and Comments related entities.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="searchText">The text to search in task titles.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="TaskEntity"/> instances whose titles match the search text.</returns>
    public async Task<(IReadOnlyCollection<TaskEntity> Items, int TotalCount)> SearchByTitleAsync(
        Guid userId,
        string searchText,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TaskEntity> query = this.DbSet.AsNoTracking()
        .Where(x => x.OwnerId == userId
            && EF.Functions.Like((string)(object)x.Title, $"%{searchText}%"));

        int totalCount = await query.CountAsync(cancellationToken);

        List<TaskEntity> items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Include(x => x.Tag)
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Deletes the tasks with the specified identifiers from the data store.
    /// </summary>
    /// <param name="taskIds">A collection of task identifiers representing the tasks to delete. Each identifier must correspond to an
    /// existing task.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of tasks deleted.</returns>
    public async Task<int> DeleteRangeAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default)
        => await this.DbSet.Where(t => taskIds.Contains(t.Id)).ExecuteDeleteAsync(cancellationToken);

    /// <summary>
    /// Deletes all overdue tasks within a specific task list.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list.</param>
    /// <param name="now">The current date and time used to determine overdue tasks.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of tasks that were successfully deleted.</returns>
    public async Task<int> DeleteOverdueTaskAsync(Guid taskListId, DateTime now, CancellationToken cancellationToken = default)
    {
        return await this.DbSet
                .Where(t => t.TaskListId == taskListId && t.DueDate.HasValue && t.DueDate < now)
                .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a specific user is the owner of a task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that returns <c>true</c> if the user is the owner of the task; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> IsTaskOwnerAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking()
            .AnyAsync(x => x.Id == taskId && x.OwnerId == userId, cancellationToken);

    /// <summary>
    /// Counts the number of tasks from a specified collection that belong to a particular user.
    /// </summary>
    /// <param name="taskIds">A collection of task identifiers to check.</param>
    /// <param name="userId">The unique identifier of the user who must own the tasks.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the number of tasks that match the provided IDs and belong to the specified user.
    /// </returns>
    public async Task<int> CountOwnedTasksAsync(IEnumerable<Guid> taskIds, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking()
            .Where(x => taskIds.Contains(x.Id) && x.OwnerId == userId)
            .CountAsync(cancellationToken);
}
