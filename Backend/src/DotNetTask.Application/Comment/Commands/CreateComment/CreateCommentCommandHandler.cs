using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Comment.Commands.CreateComment;

/// <summary>
/// Handles the creation of a new comment for a task.
/// </summary>
/// <remarks>
/// Validates the command, checks task access, creates a comment, and persists it.
/// </remarks>
public class CreateCommentCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskAccessService taskAccessService)
    : HandlerBase(unitOfWork), IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private readonly ITaskAccessService _taskAccessService = taskAccessService;

    /// <summary>
    /// Handles the specified <see cref="CreateCommentCommand"/>.
    /// </summary>
    /// <param name="command">Data required to create the comment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the ID of the created comment on success,
    /// or an error if validation fails or the user has no access.
    /// </returns>
    public async Task<Result<Guid>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        if (!await this._taskAccessService.HasAccessAsync(command.TaskId, command.UserId, cancellationToken))
        {
            return await Result<Guid>.FailureAsync(ErrorCode.InvalidOperation, TaskPolicy.AccessDeniedMessage);
        }

        CommentContent content = CommentContent.Create(command.Content);

        CommentEntity commentEntity = new(command.TaskId, command.UserId, content);

        await this.UnitOfWork.Comments.AddAsync(commentEntity, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(commentEntity.Id);
    }
}
