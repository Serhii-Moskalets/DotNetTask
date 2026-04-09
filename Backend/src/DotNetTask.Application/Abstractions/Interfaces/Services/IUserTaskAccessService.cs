using DotNetTask.Domain.Common;
using DotNetTask.Domain.Entities;

using TinyResult;

namespace DotNetTask.Application.Abstractions.Interfaces.Services;

/// <summary>
/// Provides operations for managing access of users to tasks.
/// </summary>
public interface IUserTaskAccessService
{
    /// <summary>
    /// Validates the user and task access rules before creating a new access entry.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="ownerId">The current owner ID performing the action.</param>
    /// <param name="sharedUser">The user to grant access to.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if all validation rules pass,
    /// or failure if any rule is violated.
    /// </returns>
    Task<Result<Unit>> CanGrantAccessAsync(
        Guid taskId,
        Guid ownerId,
        UserEntity? sharedUser,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the specified user already has access to the task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns><c>true</c> if the user already has access; otherwise, <c>false</c>.</returns>
    Task<bool> HasAccessAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
}
