using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Represents a command to grant a user access to a specific task.
/// </summary>
/// <param name="TaskId">The unique identifier of the task.</param>
/// <param name="OwnerId">The unique identifier of the user who owns the task and is granting access.</param>
/// <param name="Email">The email address of the user to whom access is being granted.</param>
public record CreateUserTaskAccessCommand(Guid TaskId, Guid OwnerId, string Email)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "CreateUserTaskAccess".</value>
    public string ActionName => "CreateUserTaskAccess";

    /// <inheritdoc/>
    public string GetIdentity() => this.OwnerId.ToString();
}
