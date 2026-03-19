using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;

/// <summary>
/// Handles the <see cref="RemoveTagFromTaskCommand"/> to remove a tag from an existing task.
/// </summary>
public class RemoveTagFromTaskCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<RemoveTagFromTaskCommand, Result<bool>>
{
    /// <summary>
    /// Handles the removal of a tag for a specific task and user.
    /// </summary>
    /// <param name="command">The <see cref="RemoveTagFromTaskCommand"/> containing the task and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(RemoveTagFromTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await this.UnitOfWork.Tasks.GetTaskByIdForUserAsync(command.TaskId, command.UserId, false, cancellationToken);
        if (task is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        if (task.TagId is null)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        task.SetTag(null);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
