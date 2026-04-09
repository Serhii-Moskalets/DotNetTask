using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessByIdCommand"/> to remove a user-task access entry.
/// </summary>
public class DeleteTaskAccessByIdCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTaskAccessByIdCommand, Result<bool>>
{
    /// <summary>
    /// Processes the command to delete a user-task access entry.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the task and the user ID whose access should be removed.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was deleted,
    /// or failure if the access was not found.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTaskAccessByIdCommand command, CancellationToken cancellationToken)
    {
        if (command.OwnerId == command.UserId)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, UserTaskAccessPolicy.OwnerAccessRemovalMessage);
        }

        if (!await this.UnitOfWork.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, cancellationToken))
        {
            return Result<bool>.Failure(ErrorCode.InvalidOperation, UserTaskAccessPolicy.AccessDeniedMessage);
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteByIdAsync(command.TaskId, command.UserId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
