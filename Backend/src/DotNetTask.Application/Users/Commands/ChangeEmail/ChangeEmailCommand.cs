using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ChangeEmail;

/// <summary>
/// Command to change the email for the current user.
/// </summary>
/// <param name="NewEmail">The new email address of the user.</param>
/// <param name="UserId">The unique identifier of the user whose email is being changed.</param>
public record ChangeEmailCommand(string NewEmail, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "ChangeEmail".</value>
    public string ActionName => "ChangeEmail";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
