using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.TaskList.Commands.UpdateTaskList;

/// <summary>
/// Command for updating the title of an existing task list.
/// </summary>
/// <param name="TaskListId">
/// The unique identifier of the task list to be updated.
/// </param>
/// <param name="UserId">
/// The unique identifier of the user who owns the task list.
/// </param>
/// <param name="NewTitle">
/// The new title for the task list.
/// This value will be validated and converted into a <see cref="Domain.ValueObjects.TaskListTitle"/> value object.
/// </param>
public record UpdateTaskListCommand(
    Guid TaskListId,
    Guid UserId,
    string NewTitle) : ICommand;
