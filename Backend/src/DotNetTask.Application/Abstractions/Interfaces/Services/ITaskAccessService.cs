namespace DotNetTask.Application.Abstractions.Interfaces.Services;

/// <summary>
/// Provides methods for checking user access to tasks.
/// </summary>
public interface ITaskAccessService
{
    /// <summary>
    /// Determines whether a specific user has access to a given task.
    /// A user has access if they are the owner of the task or if the task is shared with them.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>A task that returns <c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    Task<bool> HasAccessAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
}
