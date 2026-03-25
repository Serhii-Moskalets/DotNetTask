using DotNetTask.Domain.Entities;

namespace DotNetTask.Application.Abstractions.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing <see cref="UserTaskAccessEntity"/>.
/// Provides methods for querying, adding, and deleting user task access records.
/// </summary>
public interface IUserTaskAccessRepository
{
    /// <summary>
    /// Adds a new user task access record.
    /// </summary>
    /// <param name="entity">The user task access entity to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(UserTaskAccessEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user task access record by task ID and user ID.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="UserTaskAccessEntity"/> if found; otherwise, <c>null</c>.</returns>
    Task<UserTaskAccessEntity?> GetByTaskAndUserIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all user-task access entries for a specific task, including user details.
    /// This is typically used by the owner of the task to see which users have access.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns a read-only collection of <see cref="UserTaskAccessEntity"/> entries
    /// representing all users who currently have access to the task.
    /// </returns>
    Task<(IReadOnlyCollection<UserTaskAccessEntity> Items, int TotalCount)> GetUserTaskAccessByTaskIdAsync(
        Guid taskId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tasks shared with a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="TaskEntity"/> shared with the user.</returns>
    Task<(IReadOnlyCollection<UserTaskAccessEntity> Items, int TotalCount)> GetSharedTasksByUserIdAsync(
        Guid userId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user task access record exists for the given task and user.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the access record exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user task access record exists for the given user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the access record exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user task access record exists for the given task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the access record exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user task access record by task ID and user ID.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that returns the number of deleted user-task access entries.
    /// </returns>
    Task<int> DeleteByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all user task access records associated with a specific task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that returns the number of deleted user-task access entries.
    /// </returns>
    Task<int> DeleteAllByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all user task access records associated with a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that returns the number of deleted user-task access entries.
    /// </returns>
    Task<int> DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
