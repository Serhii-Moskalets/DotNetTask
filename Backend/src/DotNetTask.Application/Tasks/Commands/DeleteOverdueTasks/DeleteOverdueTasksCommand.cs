using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tasks.Commands.DeleteOverdueTasks;

/// <summary>
/// Command to delete all overdue tasks for a given task list and user.
/// </summary>
public record DeleteOverdueTasksCommand(Guid TaskListId, Guid UserId)
    : ICommand<int>;
