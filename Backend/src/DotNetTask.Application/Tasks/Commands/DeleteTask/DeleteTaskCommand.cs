using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tasks.Commands.DeleteTask;

/// <summary>
/// Represents a command to delete a single task from the system.
/// </summary>
/// <param name="TaskId">The unique identifier of the task to be deleted.</param>
/// <param name="UserId">The unique identifier of the user requesting the deletion.</param>
public record DeleteTaskCommand(Guid TaskId, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteTask".</value>
    public string ActionName => "DeleteTask";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
