using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;

/// <summary>
/// Represents a command to revoke all user access entries associated with a specific task.
/// </summary>
/// <param name="TaskId">The unique identifier of the task for which all access records will be cleared.</param>
/// <param name="UserId">The unique identifier of the owner or authorized user performing the bulk revocation.</param>
public record DeleteTaskAccessesByTaskCommand(Guid TaskId, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteTaskAccessesByTask".</value>
    public string ActionName => "DeleteTaskAccessesByTask";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
