using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;

/// <summary>
/// Represents a command to revoke all task access entries for a specific user across the entire system.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose access rights are being completely removed.</param>
public record DeleteTaskAccessesByUserCommand(Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteTaskAccessesByUser".</value>
    public string ActionName => "DeleteTaskAccessesByUser";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
