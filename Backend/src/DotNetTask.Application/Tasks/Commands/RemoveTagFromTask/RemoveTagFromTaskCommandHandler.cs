using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tasks.Commands.RemoveTagFromTask;

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
        TaskEntity? task = await this.UnitOfWork.Tasks.GetTaskByIdForUserAsync(command.TaskId, command.UserId, asNoTracking: false, cancellationToken);
        if (task is null)
        {
            return Result<bool>.Failure(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        if (task.TagId is null)
        {
            return Result<bool>.Success(true);
        }

        task.SetTag(null);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
