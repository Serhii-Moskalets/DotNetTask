using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tasks.Commands.AddTagToTask;

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
        TaskEntity? task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.TaskId, command.UserId, asNoTracking: false, cancellationToken);

        if (task is null)
        {
            return Result<bool>.Failure(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        if (task.TagId == command.TagId)
        {
            return Result<bool>.Success(true);
        }

        TagEntity? tag = await this.UnitOfWork.Tags.GetByIdAsync(command.TagId, true, cancellationToken);
        if (tag is null)
        {
            return Result<bool>.Failure(ErrorCode.NotFound, TagPolicy.NotFoundMessage);
        }

        if (tag.UserId != command.UserId)
        {
            return Result<bool>.Failure(ErrorCode.InvalidOperation, TagPolicy.DoNotHavePermission);
        }

        task.SetTag(command.TagId);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
