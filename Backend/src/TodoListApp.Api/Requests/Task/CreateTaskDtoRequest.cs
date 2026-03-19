namespace TodoListApp.Api.Requests.Task;

/// <summary>
/// Data Transfer Object for creating a new task.
/// </summary>
/// <param name="Title">The title of the task. Can be null.</param>
/// <param name="DueDate">The due date of the task. Can be null.</param>
public record CreateTaskDtoRequest(
    string Title,
    DateTime? DueDate = null);