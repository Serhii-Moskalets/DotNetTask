using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using MediatR;
using TinyResult;

namespace DotNetTask.Application.Tasks.Commands.DeleteRangeTasks;

/// <summary>
/// Handles the <see cref="DeleteRangeTasksCommand"/> by verifying ownership and deleting
/// a specified collection of tasks for a given user.
/// </summary>
public class DeleteRangeTasksCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteRangeTasksCommand, Result<int>>
{
    /// <summary>
    /// Handles the bulk deletion of tasks after ensuring the user owns all specified task identifiers.
    /// </summary>
    /// <param name="command">The <see cref="DeleteRangeTasksCommand"/> containing the collection of task IDs and user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Int32}"/> containing the count of successfully deleted tasks,
    /// or a failure result if access is denied.
    /// </returns>
    public async Task<Result<int>> Handle(DeleteRangeTasksCommand command, CancellationToken cancellationToken)
    {
        int ownedTasksCount = await this.UnitOfWork.Tasks.CountOwnedTasksAsync(command.TaskIds, command.UserId, cancellationToken);

        if (ownedTasksCount != command.TaskIds.Count())
        {
            return Result<int>.Failure(TinyResult.Enums.ErrorCode.ValidationError, TaskPolicy.AccessDeniedMessage);
        }

        int deletedTaskCount = await this.UnitOfWork.Tasks.DeleteRangeAsync(command.TaskIds, cancellationToken);

        return Result<int>.Success(deletedTaskCount);
    }
}
