using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tasks.Commands.AddTagToTask;

/// <summary>
/// Handles the <see cref="AddTagToTaskCommand"/> by associating a tag with a specific task for a user.
/// </summary>
public class AddTagToTaskCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<AddTagToTaskCommand, Result<bool>>
{
    /// <summary>
    /// Handles the <see cref="AddTagToTaskCommand"/>.
    /// </summary>
    /// <param name="command">The command containing TaskId, UserId, and TagId.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the tag was added,
    /// or failure if the task was not found.
    /// </returns>
    public async Task<Result<bool>> Handle(AddTagToTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.TaskId, command.UserId, asNoTracking: false, cancellationToken);

        if (task is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        if (task.TagId == command.TagId)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        var tag = await this.UnitOfWork.Tags.GetByIdAsync(command.TagId, true, cancellationToken);
        if (tag is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TagPolicy.NotFoundMessage);
        }

        if (tag.UserId != command.UserId)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, TagPolicy.DoNotHavePermission);
        }

        task.SetTag(command.TagId);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
