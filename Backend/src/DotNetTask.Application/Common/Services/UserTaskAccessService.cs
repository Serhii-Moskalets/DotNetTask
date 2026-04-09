using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Common.Services;

/// <summary>
/// Provides operations for managing access of users to tasks.
/// </summary>
public class UserTaskAccessService : IUserTaskAccessService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTaskAccessService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to access repositories.</param>
    public UserTaskAccessService(IUnitOfWork unitOfWork) => this._unitOfWork = unitOfWork;

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
    /// </returns>CanGrantAccess
    public async Task<Result<bool>> CanGrantAccessAsync(
        Guid taskId,
        Guid ownerId,
        UserEntity? sharedUser,
        CancellationToken cancellationToken)
    {
        if (sharedUser is null)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, UserTaskAccessPolicy.UserNotFoundMessage);
        }

        TaskEntity? task = await this._unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken: cancellationToken);

        if (task is null || task.OwnerId != ownerId)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, UserTaskAccessPolicy.TaskNotFoundOrAccessDeniedMessage);
        }

        if (task.OwnerId == sharedUser.Id)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, UserTaskAccessPolicy.CannotShareWithOwnerMessage);
        }

        if (await this.HasAccessAsync(taskId, sharedUser.Id, cancellationToken))
        {
            return Result<bool>.Failure(ErrorCode.InvalidOperation, UserTaskAccessPolicy.AlreadySharedMessage);
        }

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Checks whether the specified user already has access to the task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns><c>true</c> if the user already has access; otherwise, <c>false</c>.</returns>
    public async Task<bool> HasAccessAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default) => await this._unitOfWork.UserTaskAccesses.ExistsAsync(taskId, userId, cancellationToken);
}
