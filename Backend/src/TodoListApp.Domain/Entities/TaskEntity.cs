using TinyResult;
using TinyResult.Enums;
using TodoListApp.Domain.Common;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a task within a task list, including its owner, status, due date, and related comments.
/// </summary>
public class TaskEntity : BaseEntity
{
    private readonly HashSet<CommentEntity> _comments = new();

    private readonly HashSet<UserTaskAccessEntity> _userAccesses = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskEntity"/> class.
    /// </summary>
    /// <param name="ownerId">The ID of the user who created the task.</param>
    /// <param name="taskListId">The ID of the task list the task belongs to.</param>
    /// <param name="title">The title of the task.</param>
    /// <param name="dueDate">The due date of the task.</param>
    /// <param name="description">The Description of the task.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="dueDate"/> in the past.</exception>
    public TaskEntity(
        Guid ownerId,
        Guid taskListId,
        TaskTitle title,
        DateTime? dueDate = null,
        TaskDescription? description = null)
    {
        if (dueDate.HasValue && dueDate < DateTime.UtcNow)
        {
            throw new DomainException(TaskPolicy.InvalidDueDateMessage);
        }

        this.OwnerId = ownerId;
        this.TaskListId = taskListId;
        this.Title = title;
        this.Description = description;
        this.Status = StatusTask.NotStarted;
        this.DueDate = dueDate;
    }

    private TaskEntity() { }

    /// <summary>
    /// Gets the title of the task.
    /// </summary>
    public TaskTitle Title { get; private set; } = null!;

    /// <summary>
    /// Gets the description of the task.
    /// </summary>
    public TaskDescription? Description { get; private set; }

    /// <summary>
    /// Gets the due date of the task.
    /// </summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>
    /// Gets the status of the task.
    /// </summary>
    public StatusTask Status { get; private set; }

    /// <summary>
    /// Gets the ID of the user who owns the task.
    /// </summary>
    public Guid OwnerId { get; private init; }

    /// <summary>
    /// Gets the ID of the task list that this task belongs to.
    /// </summary>
    public Guid TaskListId { get; private init; }

    /// <summary>
    /// Gets the ID of the tag associated with the task, if any.
    /// </summary>
    public Guid? TagId { get; private set; }

    /// <summary>
    /// Gets the tag entity associated with the task.
    /// </summary>
    public virtual TagEntity? Tag { get; private set; }

    /// <summary>
    /// Gets the task list to which this task belongs.
    /// </summary>
    public virtual TaskListEntity TaskList { get; private init; } = null!;

    /// <summary>
    /// Gets the owner of the task.
    /// </summary>
    public virtual UserEntity Owner { get; private init; } = null!;

    /// <summary>
    /// Gets the collection of comments associated with this task.
    /// </summary>
    public virtual IReadOnlyCollection<CommentEntity> Comments => this._comments;

    /// <summary>
    /// Gets the collection of user accesses associated with this task.
    /// </summary>
    public virtual IReadOnlyCollection<UserTaskAccessEntity> UserAccesses => this._userAccesses;

    /// <summary>
    /// Updates the task title, description, and due date.
    /// </summary>
    /// <param name="title">The new title of the task.</param>
    /// <param name="description">
    /// The new description of the task.
    /// Passing null will remove the existing description.
    /// </param>
    /// <param name="dueDate">
    /// The new due date of the task. If null, the current due date remains unchanged.
    /// Must not be in the past.
    /// </param>
    /// <remarks>
    /// To remove the description, pass <c>null</c> for <paramref name="description"/>.
    /// To keep the title or due date unchanged, pass <c>null</c> for the corresponding parameter.
    /// </remarks>
    /// <exception cref="DomainException">Throws when <paramref name="dueDate"/> is in the past.</exception>
    public virtual void UpdateDetails(TaskTitle? title, TaskDescription? description = null, DateTime? dueDate = null)
    {
        if (dueDate.HasValue && dueDate < DateTime.UtcNow)
        {
            throw new DomainException(TaskPolicy.InvalidDueDateMessage);
        }

        this.Title = title ?? this.Title;
        this.Description = description;
        this.DueDate = dueDate ?? this.DueDate;
    }

    /// <summary>
    /// Sets or removes the tag associated with the task by ID.
    /// </summary>
    /// <param name="tagId">The ID of the tag to associate, or null to remove it.</param>
    /// <remarks>
    /// If the provided <paramref name="tagId"/> is the same as the current tag, no changes are made.
    /// Passing <c>null</c> removes the tag association.
    /// </remarks>
    public void SetTag(Guid? tagId)
    {
        if (this.TagId == tagId)
        {
            return;
        }

        this.TagId = tagId;
        if (tagId is null)
        {
            this.Tag = null;
        }
    }

    /// <summary>
    /// Changes the status of the task to the specified value.
    /// If the new status is the same as the current one, no action is taken.
    /// </summary>
    /// <param name="newStatus">The new status to apply to the task.</param>
    /// <returns>A result indicating success or failure of the status transition.</returns>
    public Result<bool> ChangeStatus(StatusTask newStatus)
    {
        if (!Enum.IsDefined(newStatus))
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, TaskPolicy.InvalidStatusMessage);
        }

        if (this.Status == newStatus)
        {
            return Result<bool>.Success(true);
        }

        if (newStatus == StatusTask.NotStarted && this.Status == StatusTask.Done)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, TaskPolicy.DoneToNotStartedMessage);
        }

        if (newStatus == StatusTask.NotStarted && this.Status == StatusTask.InProgress)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, TaskPolicy.InProgressToNotStartedMessage);
        }

        if (newStatus == StatusTask.Done && this.Status != StatusTask.InProgress)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, TaskPolicy.CompletionRequiresInProgressMessage);
        }

        this.Status = newStatus;
        return Result<bool>.Success(true);
    }
}
