using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.RevertEmailChange;

/// <summary>
/// Represents a command to revert a recent email change back to the original address.
/// </summary>
/// <param name="Token">The secure revert token provided in the security alert email.</param>
/// <param name="IpAddress">The IP address of the client making the request, used for security tracking.</param>
public record RevertEmailChangeCommand(string Token, string IpAddress)
    : ICommand<string>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "RevertEmailChange".</value>
    public string ActionName => "RevertEmailChange";

    /// <inheritdoc/>
    public string GetIdentity() => this.IpAddress;
}
