using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tag.Commands.CreateTag;

/// <summary>
/// Handles the <see cref="CreateTagCommand"/> by creating a new tag
/// for a specific user and optionally associating it with a task.
/// </summary>
public class CreateTagCommandHandler(
    IUnitOfWork unitOfWork,
    IUniqueValueService uniqueValueService)
    : HandlerBase(unitOfWork), IRequestHandler<CreateTagCommand, Result<Guid>>
{
    /// <summary>
    /// Processes the command to create a new tag.
    /// </summary>
    /// <param name="command">The command containing the user ID and name for the new tag.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}"/> indicating whether the operation was successful.</returns>
    public async Task<Result<Guid>> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var task = await this.UnitOfWork.Tasks.GetTaskByIdForUserAsync(
           command.TaskId,
           command.UserId,
           false,
           cancellationToken);

        if (task is null)
        {
            return await Result<Guid>.FailureAsync(ErrorCode.NotFound, TaskPolicy.TaskNotFoundMessage);
        }

        var tagName = await uniqueValueService.GetUniqueValueAsync(
            command.Name,
            name => TagName.Create(name),
            (vo, ct) => this.UnitOfWork.Tags.ExistsByNameAsync(vo, command.UserId, ct),
            cancellationToken);

        var tagEntity = new TagEntity(tagName, command.UserId);
        await this.UnitOfWork.Tags.AddAsync(tagEntity, cancellationToken);

        task.SetTag(tagEntity.Id);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(tagEntity.Id);
    }
}
