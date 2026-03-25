namespace DotNetTask.Application.Tasks.Dtos;

/// <summary>
/// Data Transfer Object used for updating a task.
/// Contains properties that can be modified for an existing task.
/// </summary>
public class UpdateTaskDto
{
    /// <summary>
    /// Gets the unique identifier of the task.
    /// </summary>
    public Guid TaskId { get; init; }

    /// <summary>
    /// Gets the new title of the task.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the new description of the task.
    /// Can be <c>null</c> if no description is provided.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the new due date of the task.
    /// Can be <c>null</c> if the due date is not specified.
    /// </summary>
    public DateTime? DueDate { get; init; }
}
