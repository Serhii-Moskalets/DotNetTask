using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessesByTaskCommand"/> to remove all user-task access entries for a specific task.
/// </summary>
public class DeleteTaskAccessesByTaskCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTaskAccessesByTaskCommand, Result<Unit>>
{
    /// <summary>
    /// Processes the command to delete all user-task access entries for a given task.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the task and the user ID of the task owner.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if all accesses were deleted,
    /// or failure if the task was not found or the user is not the task owner.
    /// </returns>
    public async Task<Result<Unit>> Handle(DeleteTaskAccessesByTaskCommand command, CancellationToken cancellationToken)
    {
        TaskEntity? task = await this.UnitOfWork.Tasks.GetTaskByIdForUserAsync(command.TaskId, command.UserId, cancellationToken: cancellationToken);
        if (task is null)
        {
            return Result<Unit>.Failure(ErrorCode.NotFound, UserTaskAccessPolicy.AccessDeniedMessage);
        }

        bool exist = await this.UnitOfWork.UserTaskAccesses.ExistsByTaskIdAsync(command.TaskId, cancellationToken);
        if (!exist)
        {
            return Result<Unit>.Failure(ErrorCode.InvalidOperation, UserTaskAccessPolicy.NoAccessesFoundMessage);
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteAllByTaskIdAsync(command.TaskId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
