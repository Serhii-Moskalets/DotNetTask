using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Comment.Commands.UpdateComment;

/// <summary>
/// Handles updating the text of an existing comment.
/// </summary>
public class UpdateCommentCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<UpdateCommentCommand, Result<bool>>
{
    /// <summary>
    /// Updates the comment if the user is the owner and validation passes.
    /// </summary>
    /// <param name="command">The comment ID, user ID, and new text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success if updated; otherwise, a failure result.</returns>
    public async Task<Result<bool>> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        CommentEntity? comment = await this.UnitOfWork.Comments.GetByIdAsync(command.CommentId, asNoTracking: false, cancellationToken);
        if (comment is null)
        {
            return Result<bool>.Failure(ErrorCode.NotFound, CommentPolicy.CommentNotFoundMessage);
        }

        if (comment.UserId != command.UserId)
        {
            return Result<bool>.Failure(ErrorCode.InvalidOperation, CommentPolicy.UpdateAccessDeniedMessage);
        }

        CommentContent newContent = CommentContent.Create(command.NewContent);

        Result<bool> result = comment.Update(newContent);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
