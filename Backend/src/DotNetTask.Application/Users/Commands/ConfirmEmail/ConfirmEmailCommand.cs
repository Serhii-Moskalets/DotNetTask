using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ConfirmEmail;

/// <summary>
/// Command to confirm user's email address.
/// </summary>
/// <param name="Token">The verification token from the email link.</param>
/// <param name="IpAddress">The IP address of the client making the request, used for security tracking.</param>
public record ConfirmEmailCommand(string Token, string IpAddress)
    : ICommand<bool>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "ConfirmEmail".</value>
    public string ActionName => "ConfirmEmail";

    /// <inheritdoc/>
    public string GetIdentity() => this.IpAddress;
}
