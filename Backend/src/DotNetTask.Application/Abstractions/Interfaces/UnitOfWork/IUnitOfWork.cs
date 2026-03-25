using DotNetTask.Application.Abstractions.Interfaces.Repositories;

namespace DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;

/// <summary>
/// Represents a unit of work that groups one or more operations
/// to be committed to the database as a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Gets the repository for managing comments.
    /// </summary>
    public ICommentRepository Comments { get; }

    /// <summary>
    /// Gets the repository for managing tags.
    /// </summary>
    public ITagRepository Tags { get; }

    /// <summary>
    /// Gets the repository for managing task lists.
    /// </summary>
    public ITaskListRepository TaskLists { get; }

    /// <summary>
    /// Gets the repository for managing tasks.
    /// </summary>
    public ITaskRepository Tasks { get; }

    /// <summary>
    /// Gets the repository for managing users.
    /// </summary>
    public IUserRepository Users { get; }

    /// <summary>
    /// Gets the repository for managing user task accesses.
    /// </summary>
    public IUserTaskAccessRepository UserTaskAccesses { get; }

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
