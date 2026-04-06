using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Domain.Entities;
using DotNetTask.Infrastructure.Extensions;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;

using Microsoft.EntityFrameworkCore;

namespace DotNetTask.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="UserTaskAccessEntity"/> objects.
/// Provides methods to add, delete, check existence, and retrieve user-task access records,
/// including shared tasks and access checks for specific users and task lists.
/// </summary>
public class UserTaskAccessRepository : IUserTaskAccessRepository
{
    private readonly DbSet<UserTaskAccessEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTaskAccessRepository"/> class
    /// with the specified database context.
    /// </summary>
    /// <param name="context">The database context used for data operations.</param>
    public UserTaskAccessRepository(DotNetTaskDbContext context) => this._dbSet = context.UserTaskAccesses;

    /// <summary>
    /// Adds a new user-task access entry.
    /// </summary>
    /// <param name="entity">The <see cref="UserTaskAccessEntity"/> to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    public async Task AddAsync(UserTaskAccessEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await this._dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Deletes all user-task access entries for a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns the number of deleted user-task access entries.
    /// </returns>
    public async Task<int> DeleteAllByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        if (taskId == Guid.Empty)
        {
            return 0;
        }

        return await this._dbSet
            .Where(x => x.TaskId == taskId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes all user-task access entries for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns the number of deleted user-task access entries.
    /// </returns>
    public async Task<int> DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return 0;
        }

        return await this._dbSet
        .Where(x => x.UserId == userId)
        .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a specific user-task access entry.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns the number of deleted user-task access entries.
    /// </returns>
    public async Task<int> DeleteByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (taskId == Guid.Empty || userId == Guid.Empty)
        {
            return 0;
        }

        return await this._dbSet
            .Where(x => x.TaskId == taskId && x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a user has access to a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns <c>true</c> if the user has access; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> ExistsAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (taskId == Guid.Empty || userId == Guid.Empty)
        {
            return false;
        }

        return await this._dbSet.AnyAsync(x => x.TaskId == taskId && x.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Checks whether a user task access record exists for the given user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the access record exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return false;
        }

        return await this._dbSet.AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Checks whether a user task access record exists for the given task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the access record exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        if (taskId == Guid.Empty)
        {
            return false;
        }

        return await this._dbSet.AnyAsync(x => x.TaskId == taskId, cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific user-task access entry by task ID and user ID.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns the <see cref="UserTaskAccessEntity"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public async Task<UserTaskAccessEntity?> GetByTaskAndUserIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (taskId == Guid.Empty || userId == Guid.Empty)
        {
            return null;
        }

        return await this._dbSet
            .AsNoTracking()
            .Include(x => x.Task)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(x => x.TaskId == taskId && x.UserId == userId, cancellationToken);
    }

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
    public async Task<(IReadOnlyCollection<UserTaskAccessEntity> Items, int TotalCount)> GetUserTaskAccessByTaskIdAsync(
        Guid taskId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (taskId == Guid.Empty)
        {
            return (Array.Empty<UserTaskAccessEntity>(), 0);
        }

        IQueryable<UserTaskAccessEntity> taskAccessQuery = this._dbSet.AsNoTracking()
           .Where(x => x.TaskId == taskId);

        int totalCount = await taskAccessQuery.CountAsync(cancellationToken);

        List<UserTaskAccessEntity> items = await taskAccessQuery
           .Include(x => x.User)
           .OrderBy(x => x.User.FirstName)
           .ApplyPagination(page, pageSize)
           .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Retrieves all user-task access entries for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A task that returns a read-only collection of <see cref="UserTaskAccessEntity"/>
    /// entries representing the tasks shared with the user.
    /// </returns>
    public async Task<(IReadOnlyCollection<UserTaskAccessEntity> Items, int TotalCount)> GetSharedTasksByUserIdAsync(
        Guid userId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return (Array.Empty<UserTaskAccessEntity>(), 0);
        }

        IQueryable<UserTaskAccessEntity> taskAccessQuery = this._dbSet.AsNoTracking()
            .Where(x => x.UserId == userId);

        int totalCount = await taskAccessQuery.CountAsync(cancellationToken);

        List<UserTaskAccessEntity> items = await taskAccessQuery
            .Include(x => x.Task).ThenInclude(t => t.Tag)
            .OrderByDescending(x => x.CreatedDate)
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}