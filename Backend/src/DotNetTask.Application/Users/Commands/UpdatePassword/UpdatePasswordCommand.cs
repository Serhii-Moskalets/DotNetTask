using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.UpdatePassword;

/// <summary>
/// Command to update the password for the current user.
/// </summary>
/// <param name="CurrentPassword">The user's current password for identity verification.</param>
/// <param name="NewPassword">The new password to be set.</param>
/// <param name="UserId">The unique identifier of the user whose password is being updated.</param>
public record UpdatePasswordCommand(string CurrentPassword, string NewPassword, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "UpdatePassword".</value>
    public string ActionName => "UpdatePassword";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
