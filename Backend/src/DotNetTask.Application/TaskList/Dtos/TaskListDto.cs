namespace DotNetTask.Application.TaskList.Dtos;

/// <summary>
/// Data Transfer Object representing a Task List Entity.
/// </summary>
public class TaskListDto
{
    /// <summary>
    /// Gets the unique identifier of the task list.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the task list.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Gets the identifier of the user who owns the task list.
    /// </summary>
    public Guid OwnerId { get; init; }
}
