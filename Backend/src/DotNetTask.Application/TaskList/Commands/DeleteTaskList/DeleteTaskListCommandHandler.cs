using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.TaskList.Commands.DeleteTaskList;

/// <summary>
/// Handles the <see cref="DeleteTaskListCommand"/> by deleting a task list
/// that belongs to a specific user.
/// </summary>
public class DeleteTaskListCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTaskListCommand, Result<bool>>
{
    /// <summary>
    /// Handles the command to delete a task list.
    /// </summary>
    /// <param name="command">The command containing the task list ID and user ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing <c>true</c> if the task list was successfully deleted;
    /// otherwise, a failure result with an appropriate error code.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTaskListCommand command, CancellationToken cancellationToken)
    {
        TaskListEntity? taskList = await this.UnitOfWork.TaskLists
            .GetTaskListByIdForUserAsync(command.TaskListId, command.UserId, asNoTracking: false, cancellationToken);
        if (taskList is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TaskListPolicy.NotFoundMessage);
        }

        this.UnitOfWork.TaskLists.Delete(taskList);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
