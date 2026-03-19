using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.TaskList.Commands.CreateTaskList;

/// <summary>
/// Command for creating a new task list for a specific user.
/// </summary>
/// <param name="UserId">
/// The unique identifier of the user who owns the task list.
/// </param>
/// <param name="Title">
/// The raw title of the task list.
/// This value will be validated and converted into a <see cref="Domain.ValueObjects.TaskListTitle"/> value object.
/// </param>
public record CreateTaskListCommand(
    Guid UserId,
    string Title) : ICommand<Guid>;
