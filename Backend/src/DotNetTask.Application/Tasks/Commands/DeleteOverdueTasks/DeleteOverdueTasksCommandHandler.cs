using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tasks.Commands.DeleteOverdueTasks;

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
    /// <returns>
    /// A <see cref="Result{T}"/> containing the count of successfully deleted tasks on success,
    /// or a failure result if the task list is not found.
    /// </returns>
    public async Task<Result<int>> Handle(DeleteOverdueTasksCommand command, CancellationToken cancellationToken)
    {
        TaskListEntity? taskList = await this.UnitOfWork.TaskLists
            .GetTaskListByIdForUserAsync(command.TaskListId, command.UserId, asNoTracking: true, cancellationToken);

        if (taskList is null)
        {
            return Result<int>.Failure(ErrorCode.NotFound, TaskListPolicy.NotFoundMessage);
        }

        int deletedTasksCount = await this.UnitOfWork.Tasks
            .DeleteOverdueTaskAsync(command.TaskListId, DateTime.UtcNow, cancellationToken);

        return Result<int>.Success(deletedTasksCount);
    }
}
