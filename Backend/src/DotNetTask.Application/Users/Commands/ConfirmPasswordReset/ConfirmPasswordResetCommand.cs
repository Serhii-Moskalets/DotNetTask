using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Represents a command to confirm a user's password reset.
/// </summary>
/// <param name="NewPassword">The new password to be set.</param>
/// <param name="Token">The token.</param>
/// <param name="IpAddress">The IP address of the client making the request, used for security tracking.</param>
public record ConfirmPasswordResetCommand(string NewPassword, string Token, string IpAddress)
    : ICommand<bool>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "ConfirmPasswordReset".</value>
    public string ActionName => "ConfirmPasswordReset";

    /// <inheritdoc/>
    public string GetIdentity() => this.IpAddress;
}
