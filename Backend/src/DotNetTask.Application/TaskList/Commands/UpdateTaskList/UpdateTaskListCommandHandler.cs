using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.TaskList.Commands.UpdateTaskList;

/// <summary>
/// Handles the <see cref="UpdateTaskListCommand"/> by updating the title of a task list
/// that belongs to a specific user.
/// </summary>
public class UpdateTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    IUniqueValueService uniqueNameService)
    : HandlerBase(unitOfWork), IRequestHandler<UpdateTaskListCommand, Result<bool>>
{
    /// <summary>
    /// Handles the command to update the title of a task list.
    /// </summary>
    /// <param name="command">The command containing the task list ID, user ID, and new title.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing <c>true</c> if the task list title was successfully updated;
    /// otherwise, a failure result with an appropriate error code.
    /// </returns>
    public async Task<Result<bool>> Handle(UpdateTaskListCommand command, CancellationToken cancellationToken)
    {
        TaskListEntity? taskList = await this.UnitOfWork.TaskLists
            .GetTaskListByIdForUserAsync(command.TaskListId, command.UserId, asNoTracking: false, cancellationToken);

        if (taskList is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TaskListPolicy.NotFoundMessage);
        }

        if (taskList.Title.Value == command.NewTitle)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        TaskListTitle title = await uniqueNameService.GetUniqueValueAsync(
            command.NewTitle,
            name => TaskListTitle.Create(name),
            (vo, ct) => this.UnitOfWork.TaskLists.ExistsByTitleAsync(vo, command.UserId, ct),
            cancellationToken);

        taskList.UpdateTitle(title);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
