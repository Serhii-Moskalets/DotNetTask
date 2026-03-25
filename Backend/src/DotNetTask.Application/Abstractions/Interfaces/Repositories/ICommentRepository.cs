using DotNetTask.Domain.Entities;

namespace DotNetTask.Application.Abstractions.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="CommentEntity"/>.
/// Provides methods for querying, creating, updating, and deleting comments.
/// </summary>
public interface ICommentRepository : IRepository<CommentEntity>
{
    /// <summary>
    /// Retrieves all comments associated with a specific task.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of comments per page.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A read-only collection of <see cref="CommentEntity"/>.</returns>
    Task<(IReadOnlyCollection<CommentEntity> Items, int TotalCount)> GetCommentsByTaskIdAsync(
        Guid taskId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
