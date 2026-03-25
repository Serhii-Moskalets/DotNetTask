using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tasks.Commands.ChangeTaskStatus;

/// <summary>
/// Handles the <see cref="ChangeTaskStatusCommand"/> by changing the status of a task
/// that belongs to a specific user.
/// </summary>
public class ChangeTaskStatusCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<ChangeTaskStatusCommand, Result<bool>>
{
    /// <summary>
    /// Processes the command to change the status of an existing task.
    /// </summary>
    /// <param name="command">
    /// The command containing the task identifier, user identifier,
    /// and the new status to be applied.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the result of the operation.
    /// </returns>
    public async Task<Result<bool>> Handle(ChangeTaskStatusCommand command, CancellationToken cancellationToken)
    {
        TaskEntity? task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.TaskId, command.UserId, asNoTracking: false, cancellationToken);

        if (task is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        if (task.Status == command.Status)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        task.ChangeStatus(command.Status);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
