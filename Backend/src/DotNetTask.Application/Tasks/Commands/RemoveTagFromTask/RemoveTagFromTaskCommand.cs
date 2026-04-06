using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tasks.Commands.RemoveTagFromTask;

/// <summary>
/// Represents a command to remove a specific tag association from a task.
/// </summary>
/// <param name="TaskId">The unique identifier of the task.</param>
/// <param name="UserId">The unique identifier of the user performing the removal.</param>
public record RemoveTagFromTaskCommand(Guid TaskId, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "RemoveTagFromTask".</value>
    public string ActionName => "RemoveTagFromTask";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
