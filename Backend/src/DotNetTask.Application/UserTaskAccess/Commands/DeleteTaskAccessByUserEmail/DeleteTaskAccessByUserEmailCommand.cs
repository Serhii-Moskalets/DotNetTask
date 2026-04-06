using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Represents a command to revoke a user's access to a specific task using their email address.
/// </summary>
/// <param name="TaskId">The unique identifier of the task.</param>
/// <param name="OwnerId">The unique identifier of the task owner performing the revocation.</param>
/// <param name="Email">The email address of the user whose access is being revoked.</param>
public record DeleteTaskAccessByUserEmailCommand(Guid TaskId, Guid OwnerId, string Email)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteTaskAccessByUserEmail".</value>
    public string ActionName => "DeleteTaskAccessByUserEmail";

    /// <inheritdoc/>
    public string GetIdentity() => this.OwnerId.ToString();
}
