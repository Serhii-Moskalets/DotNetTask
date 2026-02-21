using System.ComponentModel.DataAnnotations.Schema;
using TodoListApp.Domain.Common;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a user's comment on a task.
/// </summary>
[Table("comments")]
public class CommentEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommentEntity"/> class.
    /// </summary>
    /// <param name="taskId">The ID of the task the comment belongs to.</param>
    /// <param name="userId">The ID of the user who created the comment.</param>
    /// <param name="text">The text content of the comment.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="text"/> is null, empty, or consists only of white-space characters or exceed 1000 characters.
    /// </exception>
    public CommentEntity(Guid taskId, Guid userId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("Comment text cannot be empty.");
        }

        if (text.Length > 1000)
        {
            throw new DomainException("Comment text cannot exceed 1000 characters.");
        }

        this.TaskId = taskId;
        this.UserId = userId;
        this.Text = text.Trim();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentEntity"/> class
    /// with the specified task ID, user ID, comment text, and the associated user entity.
    /// </summary>
    /// <param name="taskId">The ID of the task to which this comment belongs.</param>
    /// <param name="userId">The ID of the user who created the comment.</param>
    /// <param name="text">The text content of the comment. Cannot be null or empty.</param>
    /// <param name="user">The <see cref="UserEntity"/> representing the user who created the comment.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="text"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public CommentEntity(Guid taskId, Guid userId, string text, UserEntity user)
    : this(taskId, userId, text)
    {
        if (user.Id != userId)
        {
            throw new DomainException("User ID mismatch.");
        }

        this.User = user;
    }

    private CommentEntity() { }

    /// <summary>
    /// Gets the text content of the comment.
    /// </summary>
    [Column("text")]
    public string Text { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the task that this comment belongs to.
    /// </summary>
    [Column("task_id")]
    public Guid TaskId { get; }

    /// <summary>
    /// Gets the ID of the user who created the comment.
    /// </summary>
    [Column("user_id")]
    public Guid UserId { get; }

    /// <summary>
    /// Gets the user who created the comment.
    /// </summary>
    public virtual UserEntity User { get; } = null!;

    /// <summary>
    /// Gets the task to which this comment belongs.
    /// </summary>
    public virtual TaskEntity Task { get; } = null!;

    /// <summary>
    /// Updates the text content of the comment.
    /// </summary>
    /// <param name="text">The new text content of the comment.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="text"/> is null, empty, or consists only of white-space characters or exceed 1000 characters.
    /// </exception>
    public void Update(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("Comment text cannot be empty.");
        }

        if (text.Length > 1000)
        {
            throw new DomainException("Comment text cannot exceed 1000 characters.");
        }

        this.Text = text.Trim();
    }
}
