using DotNetTask.Domain.Enums;

namespace DotNetTask.Application.Common.Dtos;

/// <summary>
/// Data Transfer Object representing a brief summary of a task.
/// Optimized for list displays where full details like description or comments are not required.
/// </summary>
public class TaskBriefDto
{
    /// <summary>
    /// Gets the unique identifier of the task.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the task.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Gets the description of the task.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the date and time when the task was created.
    /// </summary>
    public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the due date of the task.
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Gets the current status of the task.
    /// </summary>
    public StatusTask Status { get; init; }

    /// <summary>
    /// Gets the ID of the user who owns the task.
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the ID of the task list this task belongs to.
    /// </summary>
    public Guid TaskListId { get; init; }

    /// <summary>
    /// Gets the tag associated with the task, if any.
    /// </summary>
    public TagDto? Tag { get; init; }
}
