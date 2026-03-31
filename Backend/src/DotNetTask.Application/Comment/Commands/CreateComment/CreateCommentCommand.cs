using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Comment.Commands.CreateComment;

/// <summary>
/// Command for creating a new comment on a specific task.
/// </summary>
/// <param name="TaskId">The unique identifier of the task to which the comment will be attached.</param>
/// <param name="UserId">The unique identifier of the user who is creating the comment.</param>
/// <param name="Content">
/// The raw text content of the comment.
/// This value will be validated and converted into a <see cref="Domain.ValueObjects.CommentContent"/> object.
/// </param>
public record CreateCommentCommand(
    Guid TaskId,
    Guid UserId,
    string Content) : ICommand<Guid>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "CreateComment".</value>
    public string ActionName => "CreateComment";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
