using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tasks.Commands.AddTagToTask;

/// <summary>
/// Represents a command to associate a tag with a specific task.
/// </summary>
/// <param name="TaskId">The unique identifier of the task receiving the tag.</param>
/// <param name="UserId">The unique identifier of the user performing the action.</param>
/// <param name="TagId">The unique identifier of the tag to be added.</param>
public record AddTagToTaskCommand(Guid TaskId, Guid UserId, Guid TagId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "AddTagToTask".</value>
    public string ActionName => "AddTagToTask";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
