using System.ComponentModel.DataAnnotations.Schema;
using TodoListApp.Domain.Common;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a task list owned by a user, containing multiple tasks.
/// </summary>
[Table("task_lists")]
public class TaskListEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListEntity"/> class.
    /// </summary>
    /// <param name="ownerId">The ID of the user who created the task list.</param>
    /// <param name="title">The title of the task list.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="title"/> is null, empty, or consists only of white-space characters
    /// or exceed 50 characters.
    /// </exception>
    public TaskListEntity(Guid ownerId, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title of the task list cannot be empty.");
        }

        if (title.Length > 100)
        {
            throw new DomainException("Title cannot exceed 50 characters.");
        }

        this.OwnerId = ownerId;
        this.Title = title.Trim();
    }

    private TaskListEntity() { }

    /// <summary>
    /// Gets the title of the task list.
    /// </summary>
    [Column("title")]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the user who owns this task list.
    /// </summary>
    [Column("owner_id")]
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the user who owns this task list.
    /// </summary>
    public virtual UserEntity Owner { get; init; } = null!;

    /// <summary>
    /// Gets the collection of tasks contained in this task list.
    /// </summary>
    public virtual ICollection<TaskEntity> Tasks { get; init; } = new HashSet<TaskEntity>();

    /// <summary>
    /// Updates the taskList title.
    /// </summary>
    /// <param name="title">The new title of the taskList.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="title"/> is null, empty, or consists only of white-space characters
    /// or exceed 50 characters.
    /// </exception>
    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title of the task list cannot be empty.");
        }

        if (title.Length > 50)
        {
            throw new DomainException("Title cannot exceed 50 characters.");
        }

        this.Title = title.Trim();
    }
}
