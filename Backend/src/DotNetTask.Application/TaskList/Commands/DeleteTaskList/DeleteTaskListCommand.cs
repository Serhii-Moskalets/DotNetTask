using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.TaskList.Commands.DeleteTaskList;

/// <summary>
/// Represents a command to delete an entire task list and its associated contents.
/// </summary>
/// <param name="TaskListId">The unique identifier of the task list to be removed.</param>
/// <param name="UserId">The unique identifier of the user who owns or has permission to delete the list.</param>
public record DeleteTaskListCommand(Guid TaskListId, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteTaskList".</value>
    public string ActionName => "DeleteTaskList";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
