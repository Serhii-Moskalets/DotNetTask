using DotNetTask.Domain.Common;
using DotNetTask.Domain.ValueObjects;

namespace DotNetTask.Domain.Entities;

/// <summary>
/// Represents a task list owned by a user, containing multiple tasks.
/// </summary>
public class TaskListEntity : BaseEntity
{
    private readonly HashSet<TaskEntity> _tasks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListEntity"/> class.
    /// </summary>
    /// <param name="ownerId">The ID of the user who created the task list.</param>
    /// <param name="title">The title of the task list.</param>
    public TaskListEntity(Guid ownerId, TaskListTitle title)
    {
        this.OwnerId = ownerId;
        this.Title = title;
    }

    private TaskListEntity() { }

    /// <summary>
    /// Gets the title of the task list.
    /// </summary>
    public TaskListTitle Title { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the user who owns this task list.
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the user who owns this task list.
    /// </summary>
    public virtual UserEntity Owner { get; init; } = null!;

    /// <summary>
    /// Gets the collection of tasks contained in this task list.
    /// </summary>
    public virtual IReadOnlyCollection<TaskEntity> Tasks => this._tasks;

    /// <summary>
    /// Updates the taskList title.
    /// </summary>
    /// <param name="title">The new title of the taskList.</param>
    public void UpdateTitle(TaskListTitle title)
    {
        this.Title = title;
    }
}
