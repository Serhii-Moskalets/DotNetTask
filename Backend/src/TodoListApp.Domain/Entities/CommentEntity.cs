using TinyResult;
using TodoListApp.Domain.Common;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a user's comment on a task.
/// </summary>
public class CommentEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommentEntity"/> class.
    /// </summary>
    /// <param name="taskId">The ID of the task the comment belongs to.</param>
    /// <param name="userId">The ID of the user who created the comment.</param>
    /// <param name="content">The text content of the comment.</param>
    public CommentEntity(Guid taskId, Guid userId, CommentContent content)
    {
        this.TaskId = taskId;
        this.UserId = userId;
        this.Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentEntity"/> class
    /// with the specified task ID, user ID, comment text, and the associated user entity.
    /// </summary>
    /// <param name="taskId">The ID of the task to which this comment belongs.</param>
    /// <param name="userId">The ID of the user who created the comment.</param>
    /// <param name="content">The text content of the comment. Cannot be null or empty.</param>
    /// <param name="user">The <see cref="UserEntity"/> representing the user who created the comment.</param>
    public CommentEntity(Guid taskId, Guid userId, CommentContent content, UserEntity user)
    : this(taskId, userId, content)
    {
        if (user.Id != userId)
        {
            throw new DomainException(UserPolicy.UserIdMismatchMessage);
        }

        this.User = user;
    }

    private CommentEntity() { }

    /// <summary>
    /// Gets the text content of the comment.
    /// </summary>
    public CommentContent Content { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the task that this comment belongs to.
    /// </summary>
    public Guid TaskId { get; private init; }

    /// <summary>
    /// Gets the ID of the user who created the comment.
    /// </summary>
    public Guid UserId { get; private init; }

    /// <summary>
    /// Gets the user who created the comment.
    /// </summary>
    public virtual UserEntity User { get; private init; } = null!;

    /// <summary>
    /// Gets the task to which this comment belongs.
    /// </summary>
    public virtual TaskEntity Task { get; private init; } = null!;

    /// <summary>
    /// Updates the text content of the comment.
    /// </summary>
    /// <param name="content">The new text content of the comment.</param>
    /// <returns>
    /// A <see cref="Result{Boolean}"/> indicating success (true) if the content was updated,
    /// or a failure if the new content is the same as the current one.
    /// </returns>
    public Result<bool> Update(CommentContent content)
    {
        if (this.Content == content)
        {
            return Result<bool>.Failure(
                TinyResult.Enums.ErrorCode.InvalidOperation,
                CommentPolicy.NoChangesDetectedMessage);
        }

        this.Content = content;
        return Result<bool>.Success(true);
    }
}
