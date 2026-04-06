using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Represents a command to revoke a user's access to a specific task using their unique identifiers.
/// </summary>
/// <param name="TaskId">The unique identifier of the task.</param>
/// <param name="UserId">The unique identifier of the user whose access is being revoked.</param>
/// <param name="OwnerId">The unique identifier of the task owner performing the action.</param>
public record DeleteTaskAccessByIdCommand(Guid TaskId, Guid UserId, Guid OwnerId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteTaskAccessById".</value>
    public string ActionName => "DeleteTaskAccessById";

    /// <inheritdoc/>
    public string GetIdentity() => this.OwnerId.ToString();
}
