using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ConfirmEmailChange;

/// <summary>
/// Represents a command to confirm a user's email change.
/// </summary>
/// <param name="Token">The token.</param>
/// <param name="IpAddress">The IP address of the client making the request, used for security tracking.</param>
public record ConfirmEmailChangeCommand(string Token, string IpAddress)
    : ICommand<bool>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "ConfirmEmailChange".</value>
    public string ActionName => "ConfirmEmailChange";

    /// <inheritdoc/>
    public string GetIdentity() => this.IpAddress;
}
