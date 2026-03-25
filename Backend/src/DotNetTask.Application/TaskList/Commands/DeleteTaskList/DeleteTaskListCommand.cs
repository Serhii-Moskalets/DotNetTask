using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.TaskList.Commands.DeleteTaskList;

/// <summary>
/// Command to delete a specific task list for a given user.
/// </summary>
public record DeleteTaskListCommand(Guid TaskListId, Guid UserId)
    : ICommand;
