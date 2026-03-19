using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;

/// <summary>
/// Handles the <see cref="DeleteOverdueTasksCommand"/> and deletes all overdue tasks.
/// in a specified task list for a given user.
/// </summary>
public class DeleteOverdueTasksCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteOverdueTasksCommand, Result<int>>
{
    /// <summary>
    /// Handles the deletion of overdue tasks in the specified task list for a given user.
    /// </summary>
    /// <param name="command">The <see cref="DeleteOverdueTasksCommand"/> containing task list and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<int>> Handle(DeleteOverdueTasksCommand command, CancellationToken cancellationToken)
    {
        var taskList = await this.UnitOfWork.TaskLists
            .GetTaskListByIdForUserAsync(command.TaskListId, command.UserId, asNoTracking: true, cancellationToken);

        if (taskList is null)
        {
            return await Result<int>.FailureAsync(ErrorCode.NotFound, TaskListPolicy.NotFoundMessage);
        }

        var deletedTasksCount = await this.UnitOfWork.Tasks
            .DeleteOverdueTaskAsync(command.TaskListId, DateTime.UtcNow, cancellationToken);

        return await Result<int>.SuccessAsync(deletedTasksCount);
    }
}
