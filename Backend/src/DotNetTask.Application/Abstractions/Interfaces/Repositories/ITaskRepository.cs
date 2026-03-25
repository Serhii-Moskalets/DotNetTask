using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;

namespace DotNetTask.Application.Abstractions.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="TaskEntity"/>.
/// Provides methods for querying, creating, updating, and deleting tasks.
/// </summary>
public interface ITaskRepository : IRepository<TaskEntity>
{
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
    Task<(IReadOnlyCollection<TaskEntity> items, int TotalCount)> GetTasksAsync(
        Guid userId,
        Guid todoListId,
        int page = 1,
        int pageSize = 10,
        IReadOnlyCollection<StatusTask>? statuses = null,
        DateTime? dueBefore = null,
        DateTime? dueAfter = null,
        TaskSortBy? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tasks by title for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="searchText">The text to search for in task titles.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only collection of matching tasks.</returns>
    Task<(IReadOnlyCollection<TaskEntity> Items, int TotalCount)> SearchByTitleAsync(
        Guid userId,
        string searchText,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts overdue tasks for a specific user and To-Do list.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="taskListId">The To-Do list identifier.</param>
    /// <param name="now">The current date and time used to determine overdue tasks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of overdue tasks.</returns>
    Task<int> CountOverdueTasksAsync(
        Guid userId,
        Guid taskListId,
        DateTime now,
        CancellationToken cancellationToken = default);

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
    Task<TaskEntity?> GetTaskByIdForUserAsync(
        Guid taskId,
        Guid userId,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all overdue tasks within a specific task list.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list.</param>
    /// <param name="now">The current date and time used to determine overdue tasks.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of tasks that were successfully deleted.</returns>
    Task<int> DeleteOverdueTaskAsync(Guid taskListId, DateTime now, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a specific user is the owner of a task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that returns <c>true</c> if the user is the owner of the task; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsTaskOwnerAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
}