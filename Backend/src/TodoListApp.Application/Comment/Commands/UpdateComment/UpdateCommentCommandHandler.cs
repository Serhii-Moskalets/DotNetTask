using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Comment.Commands.UpdateComment;

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
        var comment = await this.UnitOfWork.Comments.GetByIdAsync(command.CommentId, false, cancellationToken);
        if (comment is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, CommentPolicy.CommentNotFoundMessage);
        }

        if (comment.UserId != command.UserId)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, CommentPolicy.UpdateAccessDeniedMessage);
        }

        var newContent = CommentContent.Create(command.NewContent);

        var result = comment.Update(newContent);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
