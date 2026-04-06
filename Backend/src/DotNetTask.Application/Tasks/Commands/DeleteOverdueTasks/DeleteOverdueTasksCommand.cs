using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tasks.Commands.DeleteOverdueTasks;

/// <summary>
/// Represents a command to bulk delete all tasks that have passed their due date.
/// </summary>
/// <param name="TaskListId">The unique identifier of the task list to clean up.</param>
/// <param name="UserId">The unique identifier of the user performing the bulk deletion.</param>
public record DeleteOverdueTasksCommand(Guid TaskListId, Guid UserId)
    : ICommand<int>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteOverdueTasks".</value>
    public string ActionName => "DeleteOverdueTasks";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
