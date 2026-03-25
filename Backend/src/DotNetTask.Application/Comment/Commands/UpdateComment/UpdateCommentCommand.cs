using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Comment.Commands.UpdateComment;

/// <summary>
/// Represents a command to update the content of an existing comment.
/// </summary>
/// <param name="CommentId">The unique identifier of the comment to be updated.</param>
/// <param name="UserId">The unique identifier of the user attempting to perform the update (for authorization checks).</param>
/// <param name="NewContent">
/// The new raw text content for the comment. This value will be validated and converted into a <see cref="Domain.ValueObjects.CommentContent"/> object.
/// </param>
public record UpdateCommentCommand(
    Guid CommentId,
    Guid UserId,
    string NewContent) : ICommand;
