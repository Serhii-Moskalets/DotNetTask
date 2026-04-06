using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Comment.Commands.DeleteComment;

/// <summary>
/// Represents a command to delete a specific comment.
/// </summary>
/// <param name="CommentId">The unique identifier of the comment to be removed.</param>
/// <param name="UserId">The unique identifier of the user attempting to delete the comment.</param>
public record DeleteCommentCommand(Guid CommentId, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteComment".</value>
    public string ActionName => "DeleteComment";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
