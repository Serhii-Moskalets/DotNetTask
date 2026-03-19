namespace TodoListApp.Api.Requests.Task;

/// <summary>
/// Represents the data required to update an existing task.
/// </summary>
/// <param name="Title">The new title of the task. Optional.</param>
/// <param name="Description">The new description of the task. Optional.</param>
/// <param name="DueDate">The new due date of the task. Optional.</param>
public record UpdateTaskDtoRequest(
    string? Title = null,
    string? Description = null,
    DateTime? DueDate = null);