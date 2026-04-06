using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Enums;

namespace DotNetTask.Application.Tasks.Commands.ChangeTaskStatus;

/// <summary>
/// Represents a command to update the current status of a specific task.
/// </summary>
/// <param name="TaskId">The unique identifier of the task to be updated.</param>
/// <param name="UserId">The unique identifier of the user performing the status change.</param>
/// <param name="Status">The new status to be applied to the task.</param>
public record ChangeTaskStatusCommand(Guid TaskId, Guid UserId, StatusTask Status)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "ChangeTaskStatus".</value>
    public string ActionName => "ChangeTaskStatus";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
