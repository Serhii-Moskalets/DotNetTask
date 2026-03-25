using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tasks.Commands.DeleteTask;

/// <summary>
/// Handles the <see cref="DeleteTaskCommand"/> to delete an existing task.
/// </summary>
public class DeleteTaskCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTaskCommand, Result<bool>>
{
    /// <summary>
    /// Handles the deletion of a task for a specific user.
    /// </summary>
    /// <param name="command">The <see cref="DeleteTaskCommand"/> containing task and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        TaskEntity? task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.TaskId, command.UserId, asNoTracking: false, cancellationToken);
        if (task is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        await this.UnitOfWork.Tasks.DeleteAsync(task, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
