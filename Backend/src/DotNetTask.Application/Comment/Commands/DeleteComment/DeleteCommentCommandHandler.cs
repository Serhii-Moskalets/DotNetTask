using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Comment.Commands.DeleteComment;

/// <summary>
/// Handles the deletion of an existing comment.
/// </summary>
public class DeleteCommentCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteCommentCommand, Result<bool>>
{
    /// <summary>
    /// Deletes the comment identified by the command if allowed.
    /// </summary>
    /// <param name="command">Comment ID and user ID requesting deletion.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success if deleted; otherwise, a failure result.</returns>
    public async Task<Result<bool>> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        CommentEntity? comment = await this.UnitOfWork.Comments.GetByIdAsync(command.CommentId, asNoTracking: true, cancellationToken);
        if (comment is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, CommentPolicy.CommentNotFoundMessage);
        }

        bool isCommentOwner = command.UserId == comment.UserId;
        bool isTaskOwner = await this.UnitOfWork.Tasks.IsTaskOwnerAsync(comment.TaskId, command.UserId, cancellationToken);

        if (!isCommentOwner && !isTaskOwner)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, CommentPolicy.DeleteAccessDeniedMessage);
        }

        await this.UnitOfWork.Comments.DeleteAsync(comment, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
