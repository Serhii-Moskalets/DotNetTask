namespace TodoListApp.Application.Tasks.Dtos;

/// <summary>
/// Data Transfer Object for creating a new task.
/// </summary>
public class CreateTaskDto
{
    /// <summary>
    /// Gets the title of the task.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Gets the due date of the task.
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Gets the ID of the task list this task belongs to.
    /// </summary>
    public Guid TaskListId { get; init; }
}
